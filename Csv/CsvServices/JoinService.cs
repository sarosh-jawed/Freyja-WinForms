using Freyja.Models.Combined;
using Freyja.Models.Normalized;

namespace Freyja.Csv.CsvServices;

public sealed class JoinService
{
    public JoinResult BuildCombinedRecords(NormalizationResult norm)
    {
        var result = new JoinResult
        {
            TotalEventsInput = norm.EventsReadyForJoin.Count
        };

        // Key: (ExternalSystemId/UserBarcode, ItemBarcode)
        var combinedMap = new Dictionary<(string userId, string itemBarcode), CombinedRecord>();

        foreach (var ev in norm.EventsReadyForJoin)
        {
            // In your Phase 3 model these are required strings (non-null)
            var userId = ev.UserBarcode.Trim();
            var itemBarcode = ev.ItemBarcode.Trim();

            // Defensive checks (shouldn't happen if Phase 3 is correct, but keep safe)
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(itemBarcode))
            {
                result.Errors.Add(new JoinError
                {
                    UserExternalSystemId = userId,
                    ItemBarcode = itemBarcode,
                    Reason = "Missing join key fields on event"
                });
                continue;
            }

            // Lookup user
            if (!norm.UsersByExternalId.TryGetValue(userId, out var user))
            {
                result.MissingUsers++;
                result.Errors.Add(new JoinError
                {
                    UserExternalSystemId = userId,
                    ItemBarcode = itemBarcode,
                    Reason = "User not found in Users lookup (external_system_id)"
                });
                continue;
            }

            // Lookup item
            if (!norm.ItemsByBarcode.TryGetValue(itemBarcode, out var item))
            {
                result.MissingItems++;
                result.Errors.Add(new JoinError
                {
                    UserExternalSystemId = userId,
                    ItemBarcode = itemBarcode,
                    Reason = "Item barcode not found in ItemMatched lookup"
                });
                continue;
            }

            var key = (userId, itemBarcode);

            if (!combinedMap.TryGetValue(key, out var combined))
            {
                combined = new CombinedRecord
                {
                    UserExternalSystemId = userId,
                    ItemBarcode = itemBarcode,

                    // Your normalized user uses DisplayName
                    UserFullName = user.DisplayName,
                    PatronGroup = user.PatronGroup,

                    // Your normalized item uses Instance
                    InstanceSummary = item.Instance,
                    MaterialType = item.MaterialType
                };

                combinedMap[key] = combined;
            }

            combined.Charges.Add(new CombinedCharge
            {
                FeeFineType = ev.FeeFineType,
                Amount = ev.Amount,
                Date = ev.BilledAt,

                // Not in your NormalizedCirculationEvent currently
                ServicePoint = "",
                Source = ""
            });

            result.EventsJoined++;
        }

        result.Records.AddRange(combinedMap.Values);
        return result;
    }
}
