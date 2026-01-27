namespace Freyja.Models.Normalized;

public sealed class NormalizedItem
{
    public required string Barcode { get; init; }
    public required string Instance { get; init; }
    public required string MaterialType { get; init; }
}
