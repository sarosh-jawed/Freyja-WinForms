using Freyja.Models.Classification;
using Freyja.Models.Combined;

namespace Freyja.Services;

public sealed class ClassificationService
{
    public ClassificationResult Classify(
        IReadOnlyList<CombinedRecord> combinedRecords,
        decimal threshold,
        IEnumerable<string> selectedPatronGroups)
    {
        var selectedGroupsSet = new HashSet<string>(
            selectedPatronGroups
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim()),
            StringComparer.OrdinalIgnoreCase);

        var forgiven = new List<ClassifiedLineItem>();
        var fine = new List<ClassifiedLineItem>();

        var chargesProcessed = 0;

        foreach (var record in combinedRecords)
        {
            // If no groups are selected, this will always be false (threshold-only mode).
            var patronGroup = record.PatronGroup?.Trim() ?? string.Empty;
            var isAutoForgiveGroup = selectedGroupsSet.Count > 0 && selectedGroupsSet.Contains(patronGroup);

            var forgivenCharges = new List<ClassifiedCharge>();
            var fineCharges = new List<ClassifiedCharge>();

            foreach (var ch in record.Charges)
            {
                chargesProcessed++;

                // OR logic:
                // - If user is in selected patron group => forgive regardless of amount
                // - Else forgive only if Amount <= threshold
                var forgive = isAutoForgiveGroup || ch.Amount <= threshold;

                var classified = new ClassifiedCharge
                {
                    FeeFineType = ch.FeeFineType?.Trim() ?? string.Empty,
                    Amount = ch.Amount,
                    BilledAt = ch.Date
                };

                if (forgive) forgivenCharges.Add(classified);
                else fineCharges.Add(classified);
            }

            // Same user+item can create a forgiven line-item and/or a fine line-item
            if (forgivenCharges.Count > 0)
            {
                forgiven.Add(new ClassifiedLineItem
                {
                    UserBarcode = record.UserExternalSystemId,
                    DisplayName = record.UserFullName,
                    PatronGroup = patronGroup,
                    ItemBarcode = record.ItemBarcode,
                    Instance = record.InstanceSummary,
                    MaterialType = record.MaterialType,
                    Charges = forgivenCharges
                });
            }

            if (fineCharges.Count > 0)
            {
                fine.Add(new ClassifiedLineItem
                {
                    UserBarcode = record.UserExternalSystemId,
                    DisplayName = record.UserFullName,
                    PatronGroup = patronGroup,
                    ItemBarcode = record.ItemBarcode,
                    Instance = record.InstanceSummary,
                    MaterialType = record.MaterialType,
                    Charges = fineCharges
                });
            }
        }

        // Optional but recommended: stable ordering for predictable output files
        forgiven = forgiven
            .OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.ItemBarcode, StringComparer.OrdinalIgnoreCase)
            .ToList();

        fine = fine
            .OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.ItemBarcode, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new ClassificationResult
        {
            ForgivenLineItems = forgiven,
            FineLineItems = fine,
            ChargesProcessed = chargesProcessed,
            RecordsProcessed = combinedRecords.Count,
            ThresholdUsed = threshold,
            SelectedPatronGroups = selectedGroupsSet.OrderBy(s => s).ToList()
        };
    }
}
