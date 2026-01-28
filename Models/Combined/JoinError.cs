namespace Freyja.Models.Combined;

public sealed class JoinError
{
    public string UserExternalSystemId { get; init; } = "";
    public string ItemBarcode { get; init; } = "";
    public string Reason { get; init; } = "";  // "Missing item barcode in ItemMatched", etc.
}
