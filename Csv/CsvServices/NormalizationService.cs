using System.Globalization;
using Freyja.Models;
using Freyja.Models.Normalized;

namespace Freyja.Csv.CsvServices;

public sealed class NormalizationService
{
    private readonly Action<string> _log;

    public NormalizationService(Action<string> log)
    {
        _log = log;
    }

    public NormalizationResult Normalize(
        IReadOnlyList<CirculationLogRow> circulation,
        IReadOnlyList<NewUserRow> users,
        IReadOnlyList<ItemMatchedRow> items)
    {
        _log("Phase 3: Normalizing and extracting fields...");

        var errors = new List<NormalizationError>();

        // 1) Build Users lookup
        var usersById = new Dictionary<string, NormalizedUser>(StringComparer.OrdinalIgnoreCase);
        foreach (var u in users)
        {
            var id = NormalizeUserBarcode(u.ExternalSystemId);
            if (string.IsNullOrWhiteSpace(id))
                continue;

            var patronGroup = (u.PatronGroup ?? "").Trim();
            var first = (u.FirstName ?? "").Trim();
            var last = (u.LastName ?? "").Trim();
            var display = $"{first} {last}".Trim();

            if (string.IsNullOrWhiteSpace(patronGroup))
                patronGroup = "Unknown";

            if (string.IsNullOrWhiteSpace(display))
                display = id;

            // If duplicates exist, last wins (fine for now)
            usersById[id] = new NormalizedUser
            {
                ExternalSystemId = id,
                PatronGroup = patronGroup,
                DisplayName = display
            };
        }
        _log($"Phase 3: Users normalized: {usersById.Count}");

        // 2) Build Items lookup
        var itemsByBarcode = new Dictionary<string, NormalizedItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var it in items)
        {
            var barcode = NormalizeItemBarcode(it.Barcode);
            if (string.IsNullOrWhiteSpace(barcode))
                continue;

            var instance = (it.Instance ?? "").Trim();
            var material = (it.MaterialType ?? "").Trim();

            if (string.IsNullOrWhiteSpace(instance))
                instance = "Unknown instance";
            if (string.IsNullOrWhiteSpace(material))
                material = "Unknown material";

            itemsByBarcode[barcode] = new NormalizedItem
            {
                Barcode = barcode,
                Instance = instance,
                MaterialType = material
            };
        }
        _log($"Phase 3: Items normalized: {itemsByBarcode.Count}");

        // 3) Normalize circulation events (filter E1, extract type/amount)
        var eventsReady = new List<NormalizedCirculationEvent>();
        int removedInvalidUserBarcode = 0;

        foreach (var r in circulation)
        {
            var userBarcode = NormalizeUserBarcode(r.UserBarcode);
            if (!IsValidUserBarcode(userBarcode))
            {
                removedInvalidUserBarcode++;
                errors.Add(new NormalizationError
                {
                    Type = NormalizationErrorType.InvalidUserBarcode,
                    UserBarcode = r.UserBarcode,
                    ItemBarcode = r.ItemBarcode,
                    Message = $"Dropped row due to invalid user barcode: '{r.UserBarcode ?? ""}'"
                });
                continue;
            }

            var itemBarcode = NormalizeItemBarcode(r.ItemBarcode);

            // Extract from Description
            if (!CirculationDescriptionParser.TryParse(r.Description, out var feeType, out var owner, out var amount, out var parseErr))
            {
                errors.Add(new NormalizationError
                {
                    Type = parseErr.Contains("Amount", StringComparison.OrdinalIgnoreCase)
                        ? NormalizationErrorType.BadAmountParse
                        : NormalizationErrorType.BadDescriptionParse,
                    UserBarcode = userBarcode,
                    ItemBarcode = itemBarcode,
                    Message = $"Description parse failed: {parseErr}"
                });
                continue;
            }

            // Date parse (optional)
            DateTime? billedAt = null;
            if (!string.IsNullOrWhiteSpace(r.DateRaw))
            {
                if (DateTime.TryParse(r.DateRaw.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    billedAt = dt;
            }

            // Missing user coverage
            if (!usersById.ContainsKey(userBarcode))
            {
                errors.Add(new NormalizationError
                {
                    Type = NormalizationErrorType.MissingUser,
                    UserBarcode = userBarcode,
                    ItemBarcode = itemBarcode,
                    Message = $"User not found in New Users file for external_system_id '{userBarcode}'."
                });
                continue;
            }

            // Missing item coverage (this is the one you want logged for ErrorLog)
            if (string.IsNullOrWhiteSpace(itemBarcode) || !itemsByBarcode.ContainsKey(itemBarcode))
            {
                errors.Add(new NormalizationError
                {
                    Type = NormalizationErrorType.MissingItem,
                    UserBarcode = userBarcode,
                    ItemBarcode = itemBarcode,
                    Message = $"Item barcode '{itemBarcode}' not found in ItemMatched file."
                });
                continue;
            }

            eventsReady.Add(new NormalizedCirculationEvent
            {
                UserBarcode = userBarcode,
                ItemBarcode = itemBarcode,
                BilledAt = billedAt,
                FeeFineType = feeType,
                FeeFineOwner = owner,
                Amount = amount,
                RawDescription = r.Description ?? ""
            });
        }

        _log($"Phase 3: Dropped invalid user barcode rows: {removedInvalidUserBarcode}");
        _log($"Phase 3: Events ready for join: {eventsReady.Count}");
        _log($"Phase 3: Errors collected: {errors.Count}");
        _log("Phase 3 complete: Normalization and field extraction successful.");
        _log("Next: Phase 4 will join datasets and build CombinedRecord list.");

        return new NormalizationResult
        {
            UsersByExternalId = usersById,
            ItemsByBarcode = itemsByBarcode,
            EventsReadyForJoin = eventsReady,
            Errors = errors
        };
    }

    private static bool IsValidUserBarcode(string? userBarcode)
        => !string.IsNullOrWhiteSpace(userBarcode)
           && userBarcode.StartsWith("E1", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeUserBarcode(string? raw)
        => (raw ?? "").Trim();

    // Handles common issues: whitespace, accidental decimals, scientific notation if it ever appears.
    private static string NormalizeItemBarcode(string? raw)
    {
        var s = (raw ?? "").Trim();
        if (string.IsNullOrWhiteSpace(s)) return "";

        // If already digits, keep as is
        if (s.All(char.IsDigit)) return s;

        // Try parse scientific notation / numeric
        if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
        {
            // Convert to integer-like string without decimals
            var asLong = (long)Math.Round(d, 0, MidpointRounding.AwayFromZero);
            return asLong.ToString(CultureInfo.InvariantCulture);
        }

        // Fallback: strip non-digits
        var digits = new string(s.Where(char.IsDigit).ToArray());
        return digits;
    }
}
