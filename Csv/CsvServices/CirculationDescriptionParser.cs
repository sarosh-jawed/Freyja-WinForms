using System.Globalization;
using System.Text.RegularExpressions;

namespace Freyja.Csv.CsvServices;

public static class CirculationDescriptionParser
{
    //BadDescriptionParse if the FOLIO changes the string in future releases.
    // Example:
    // "Fee/Fine type: Overdue fine. Fee/Fine owner: WAWL. Amount: 10.00. automated. Additional information..."
    private static readonly Regex Pattern = new(
        @"Fee/Fine type:\s*(?<type>[^.]+)\.\s*Fee/Fine owner:\s*(?<owner>[^.]+)\.\s*Amount:\s*(?<amount>[0-9]+(\.[0-9]+)?)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
    );

    public static bool TryParse(string? description, out string feeFineType, out string? owner, out decimal amount, out string error)
    {
        feeFineType = "";
        owner = null;
        amount = 0m;
        error = "";

        if (string.IsNullOrWhiteSpace(description))
        {
            error = "Description is empty.";
            return false;
        }

        var match = Pattern.Match(description);
        if (!match.Success)
        {
            error = "Description does not match expected pattern.";
            return false;
        }

        feeFineType = match.Groups["type"].Value.Trim();
        owner = match.Groups["owner"].Value.Trim();

        var amountRaw = match.Groups["amount"].Value.Trim();
        if (!decimal.TryParse(amountRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
        {
            error = $"Amount parse failed: '{amountRaw}'.";
            return false;
        }

        return true;
    }
}
