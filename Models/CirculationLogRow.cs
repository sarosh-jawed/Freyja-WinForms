namespace Freyja.Models;

public sealed class CirculationLogRow
{
    public string? UserBarcode { get; set; }
    public string? ItemBarcode { get; set; }
    public string? DateRaw { get; set; }          // keep raw; parse in normalization
    public string? Description { get; set; }      // contains Fee/Fine type + Amount
}