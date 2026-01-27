namespace Freyja.Models.Normalized;

public sealed class NormalizedCirculationEvent
{
    public required string UserBarcode { get; init; }
    public required string ItemBarcode { get; init; }

    public DateTime? BilledAt { get; init; }     // parsed from DateRaw if possible
    public required string FeeFineType { get; init; }
    public string? FeeFineOwner { get; init; }
    public required decimal Amount { get; init; }

    public required string RawDescription { get; init; }
}
