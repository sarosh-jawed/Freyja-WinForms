namespace Freyja.Models.Classification;

public sealed record ClassifiedCharge
{
    public required string FeeFineType { get; init; }
    public required decimal Amount { get; init; }
    public DateTime? BilledAt { get; init; } // nullable if you ever allow missing dates
}
