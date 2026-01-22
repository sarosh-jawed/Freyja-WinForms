namespace Freyja.Models;

public sealed class ItemMatchedRow
{
    public string Barcode { get; set; } = "";
    public string InstanceSummary { get; set; } = "";   // "Instance (Title, Publisher, Publication date)"
}
