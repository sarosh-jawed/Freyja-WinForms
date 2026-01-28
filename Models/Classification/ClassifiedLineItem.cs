using System.Globalization;

namespace Freyja.Models.Classification;

public sealed class ClassifiedLineItem
{
    public required string UserBarcode { get; init; }
    public required string DisplayName { get; init; }
    public required string PatronGroup { get; init; }

    public required string ItemBarcode { get; init; }
    public required string Instance { get; init; }
    public required string MaterialType { get; init; }

    public required List<ClassifiedCharge> Charges { get; init; }

    public decimal TotalAmount => Charges.Sum(c => c.Amount);

    public string FeeFineTypeSummary =>
        string.Join(" & ",
            Charges
                .Select(c => c.FeeFineType?.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase));

    public string TotalAmountCurrency() =>
        TotalAmount.ToString("0.00", CultureInfo.InvariantCulture);
}
