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
        // 1) Pull errors + normalize keys for dedupe + stable output
        var missingItemErrors = phase3Result.Errors
            .Where(e => e.Type == NormalizationErrorType.MissingItem)
            .Select(e => new
            {
                UserKey = Safe(e.UserBarcode, "").Trim(),
                ItemKey = CleanBarcodeForOutput(e.ItemBarcode),
                Raw = e
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.UserKey) || !string.IsNullOrWhiteSpace(x.ItemKey))
            .GroupBy(x => $"{x.UserKey}|{x.ItemKey}", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(x =>
            {
                // sort by display name if possible (staff-friendly), otherwise by user key
                if (phase3Result.UsersByExternalId.TryGetValue(x.UserKey, out var u))
                    return Safe(u.DisplayName, x.UserKey);
                return x.UserKey;
            }, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.UserKey, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.ItemKey, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var invalidUserErrors = phase3Result.Errors
            .Where(e => e.Type == NormalizationErrorType.InvalidUserBarcode)
            .Select(e => new
            {
                UserKey = Safe(e.UserBarcode, "").Trim(),
                ItemKey = CleanBarcodeForOutput(e.ItemBarcode),
                Raw = e
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.UserKey) || !string.IsNullOrWhiteSpace(x.ItemKey))
            .GroupBy(x => $"{x.UserKey}|{x.ItemKey}", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(x => x.UserKey, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.ItemKey, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // 2) Header
        var lines = new List<string>();
        lines.Add($"Freyja Error Log — generated {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        lines.Add("");

        if (missingItemErrors.Count == 0 && invalidUserErrors.Count == 0)
        {
            lines.Add("No errors.");
            return lines;
        }

        // 3) Missing items section
        if (missingItemErrors.Count > 0)
        {
            lines.Add($"Missing Item Barcodes (not found in ItemMatched file): {missingItemErrors.Count}");
            foreach (var x in missingItemErrors)
            {
                var userBarcode = string.IsNullOrWhiteSpace(x.UserKey) ? "(Unknown user)" : x.UserKey;
                var itemBarcode = string.IsNullOrWhiteSpace(x.ItemKey) ? "(Unknown item)" : x.ItemKey;

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

            lines.Add("");
        }

        // 4) Invalid users section
        if (invalidUserErrors.Count > 0)
        {
            lines.Add($"Invalid User Barcodes (dropped): {invalidUserErrors.Count}");
            foreach (var x in invalidUserErrors)
            {
                var userBarcode = string.IsNullOrWhiteSpace(x.UserKey) ? "(Unknown user)" : x.UserKey;
                var itemBarcode = x.ItemKey;

                if (!string.IsNullOrWhiteSpace(itemBarcode))
                    lines.Add($"• {userBarcode}: dropped (item {itemBarcode}).");
                else
                    lines.Add($"• {userBarcode}: dropped.");
            }
        }

        // Optional: staff hint if we see barcode formatting problems
        var sawScientific = phase3Result.Errors.Any(e => (e.ItemBarcode ?? "").IndexOf('E') >= 0 || (e.ItemBarcode ?? "").IndexOf('e') >= 0);
        var sawDecimalDot = phase3Result.Errors.Any(e => (e.ItemBarcode ?? "").EndsWith(".00", StringComparison.OrdinalIgnoreCase));
        if (sawScientific || sawDecimalDot)
        {
            lines.Add("");
            lines.Add("Note: Some item barcodes appear formatted as numbers (e.g., .00 or scientific notation).");
            lines.Add("Best practice: export CSV with barcodes as TEXT to preserve exact values.");
        }

        return lines;
    }

    private static string CleanBarcodeForOutput(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        var s = raw.Trim();

        // common artifact: 34241002514248.00
        if (s.EndsWith(".00", StringComparison.OrdinalIgnoreCase))
            s = s[..^3];

        // if the remaining is digits, we’re done
        if (IsDigitsOnly(s))
            return s;

        // if looks like scientific notation, expand it to digits for readability
        // NOTE: if the CSV itself has scientific notation, original digits may be unrecoverable.
        if (LooksLikeScientific(s) && TryExpandScientificToDigits(s, out var expanded))
            return expanded;

        // last resort: strip a trailing .0 / .00... if it’s purely numeric-ish
        // (don’t get too aggressive; keep original if unsure)
        if (TryStripDecimalZeros(s, out var stripped))
            return stripped;

        return s;
    }

    private static bool IsDigitsOnly(string s)
    {
        for (int i = 0; i < s.Length; i++)
            if (!char.IsDigit(s[i]))
                return false;
        return s.Length > 0;
    }

    private static bool LooksLikeScientific(string s)
        => s.IndexOf('E') >= 0 || s.IndexOf('e') >= 0;

    private static bool TryStripDecimalZeros(string s, out string result)
    {
        result = s;

        // e.g. "34241002514248.0000"
        var dot = s.IndexOf('.');
        if (dot < 0) return false;

        var left = s[..dot];
        var right = s[(dot + 1)..];

        if (!IsDigitsOnly(left)) return false;
        if (!right.All(ch => ch == '0')) return false;

        result = left;
        return true;
    }

    // Expands strings like "3.4241E+13" to "34241000000000"
    private static bool TryExpandScientificToDigits(string s, out string digits)
    {
        digits = string.Empty;

        // Normalize
        s = s.Trim();
        var ePos = s.IndexOfAny(new[] { 'E', 'e' });
        if (ePos < 0) return false;

        var mantissaPart = s[..ePos].Trim();
        var expPart = s[(ePos + 1)..].Trim();

        if (!int.TryParse(expPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var exp))
            return false;

        // Mantissa: allow leading sign (rare)
        var sign = "";
        if (mantissaPart.StartsWith("+"))
            mantissaPart = mantissaPart[1..];
        else if (mantissaPart.StartsWith("-"))
        {
            sign = "-";
            mantissaPart = mantissaPart[1..];
        }

        // Split mantissa around '.'
        var dot = mantissaPart.IndexOf('.');
        string whole = dot >= 0 ? mantissaPart[..dot] : mantissaPart;
        string frac = dot >= 0 ? mantissaPart[(dot + 1)..] : "";

        if (!IsDigitsOnly(whole) || (frac.Length > 0 && !IsDigitsOnly(frac)))
            return false;

        // Remove decimal point by concatenating
        var allDigits = whole + frac;

        // Decimal shifts right by exp positions relative to decimal point
        // Original decimal places = frac.Length, so final shift = exp - frac.Length
        var shift = exp - frac.Length;

        if (shift >= 0)
        {
            digits = sign + allDigits + new string('0', shift);
            // strip any leading '+' or whitespace (sign kept if negative)
            return true;
        }

        // shift < 0 would mean decimals remain; for barcodes we don’t want that
        // We’ll refuse rather than invent digits.
        return false;
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
