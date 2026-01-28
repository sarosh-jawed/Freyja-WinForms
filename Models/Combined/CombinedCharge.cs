namespace Freyja.Models.Combined;

public sealed class CombinedCharge
{
    public string FeeFineType { get; init; } = "";
    public decimal Amount { get; init; }
    public DateTime? Date { get; init; }  // optional
    public string ServicePoint { get; init; } = ""; // optional
    public string Source { get; init; } = "";       // optional
}