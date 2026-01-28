using System.Globalization;
using System.Text;
using Freyja.Models.Classification;
using Freyja.Models.Normalized;
using Freyja.Models.Output;

namespace Freyja.Services;

public sealed class OutputGenerationService
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public OutputGenerationResult Generate(
        ClassificationResult classification,
        NormalizationResult phase3Result,
        string outputFolder)
    {
        if (string.IsNullOrWhiteSpace(outputFolder))
            throw new ArgumentException("Output folder is required.", nameof(outputFolder));

        Directory.CreateDirectory(outputFolder);

        var forgivenPath = Path.Combine(outputFolder, "Forgiven_List.txt");
        var finePath = Path.Combine(outputFolder, "Fine_List.txt");
        var errorLogPath = Path.Combine(outputFolder, "ErrorLog.txt");

        var forgivenLines = BuildLines(classification.ForgivenLineItems);
        var fineLines = BuildLines(classification.FineLineItems);
        var errorLines = BuildErrorLines(phase3Result);

        WriteAllLinesAtomic(forgivenPath, forgivenLines);
        WriteAllLinesAtomic(finePath, fineLines);
        WriteAllLinesAtomic(errorLogPath, errorLines);

        return new OutputGenerationResult
        {
            ForgivenPath = forgivenPath,
            FinePath = finePath,
            ErrorLogPath = errorLogPath,
            ForgivenLinesWritten = forgivenLines.Count,
            FineLinesWritten = fineLines.Count,
            ErrorLinesWritten = errorLines.Count
        };
    }

    // -------------------------
    // Build list file lines
    // -------------------------
    private static List<string> BuildLines(IReadOnlyList<ClassifiedLineItem> items)
    {
        var lines = new List<string>(items.Count);

        foreach (var li in items)
        {
            var name = Safe(li.DisplayName, "(Unknown name)");
            var userBarcode = Safe(li.UserBarcode, "(Unknown barcode)");
            var instance = Safe(li.Instance, "(Unknown title)");
            var material = li.MaterialType?.Trim();

            // Group charges by fee type (so duplicates collapse)
            var feeGroups = li.Charges
                .GroupBy(c => Safe(c.FeeFineType, "Unknown fee type"), StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    FeeType = g.Key.Trim(),
                    Total = g.Sum(x => x.Amount)
                })
                .OrderBy(g => g.FeeType, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var total = feeGroups.Sum(g => g.Total);
            var totalText = total.ToString("0.00", CultureInfo.InvariantCulture);

            var feeTypeText = string.Join(" & ", feeGroups.Select(g => g.FeeType));

            var materialSuffix = string.IsNullOrWhiteSpace(material) ? "" : $" ({material})";

            // Final line
            lines.Add($"• {name} - {userBarcode}: $ {totalText} for {feeTypeText} for {instance}{materialSuffix}");
        }

        return lines;
    }

    // -------------------------
    // Error log lines
    // -------------------------
    private static List<string> BuildErrorLines(NormalizationResult phase3Result)
    {
        // Keep it simple: include MissingItem + InvalidUserBarcode (if you store it in Errors)
        // At minimum you must include MissingItem.
        var missingItemErrors = phase3Result.Errors
            .Where(e => e.Type == NormalizationErrorType.MissingItem)
            .OrderBy(e => e.UserBarcode, StringComparer.OrdinalIgnoreCase)
            .ThenBy(e => e.ItemBarcode, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // If you have InvalidUserBarcode captured in Errors, include them too (optional)
        var invalidUserErrors = phase3Result.Errors
            .Where(e => e.Type == NormalizationErrorType.InvalidUserBarcode)
            .OrderBy(e => e.UserBarcode, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var lines = new List<string>();

        if (missingItemErrors.Count == 0 && invalidUserErrors.Count == 0)
        {
            lines.Add("No errors.");
            return lines;
        }

        if (missingItemErrors.Count > 0)
        {
            lines.Add("Missing Item Barcodes (not found in ItemMatched file):");
            foreach (var err in missingItemErrors)
            {
                var userBarcode = Safe(err.UserBarcode, "(Unknown user)");
                var itemBarcode = Safe(err.ItemBarcode, "(Unknown item)");

                // Try to add user details if we have them
                if (phase3Result.UsersByExternalId.TryGetValue(userBarcode, out var user))
                {
                    var displayName = Safe(user.DisplayName, "(Unknown name)");
                    var group = Safe(user.PatronGroup, "(Unknown group)");
                    lines.Add($"• {displayName} - {userBarcode} ({group}): item barcode {itemBarcode} not found in ItemMatched file.");
                }
                else
                {
                    lines.Add($"• {userBarcode}: item barcode {itemBarcode} not found in ItemMatched file.");
                }
            }

            lines.Add(""); // spacer line
        }

        if (invalidUserErrors.Count > 0)
        {
            lines.Add("Invalid User Barcodes (dropped):");
            foreach (var err in invalidUserErrors)
            {
                var userBarcode = Safe(err.UserBarcode, "(Unknown user)");
                var itemBarcode = Safe(err.ItemBarcode, "");
                if (!string.IsNullOrWhiteSpace(itemBarcode))
                    lines.Add($"• {userBarcode}: dropped (item {itemBarcode}).");
                else
                    lines.Add($"• {userBarcode}: dropped.");
            }
        }

        return lines;
    }

    // -------------------------
    // Atomic write (temp -> move)
    // -------------------------
    private static void WriteAllLinesAtomic(string path, IReadOnlyList<string> lines)
    {
        var tmp = path + ".tmp";

        File.WriteAllLines(tmp, lines, Utf8NoBom);

        // Overwrite existing output safely
        File.Move(tmp, path, overwrite: true);
    }

    private static string Safe(string? s, string fallback)
        => string.IsNullOrWhiteSpace(s) ? fallback : s.Trim();
}
