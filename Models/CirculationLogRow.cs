namespace Freyja.Models;

public sealed class CirculationLogRow
{
    public string UserBarcode { get; set; } = "";
    public string ItemBarcode { get; set; } = "";
    public string Object { get; set; } = "";
    public string CircAction { get; set; } = "";
    public string DateRaw { get; set; } = "";          // will parse later if needed
    public string ServicePoint { get; set; } = "";
    public string Source { get; set; } = "";
    public string Description { get; set; } = "";
}