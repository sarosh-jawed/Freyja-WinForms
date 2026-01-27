namespace Freyja.Models;

public sealed class ItemMatchedRow
{
    public string? Barcode { get; set; }
    public string? Instance { get; set; }       // Instance (Title, Publisher, Publication date)
    public string? MaterialType { get; set; }   // Material type
}
