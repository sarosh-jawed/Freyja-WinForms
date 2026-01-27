using CsvHelper.Configuration;
using Freyja.Models;

namespace Freyja.Csv.CsvMaps;

public sealed class CirculationLogMap : ClassMap<CirculationLogRow>
{
    public CirculationLogMap()
    {
        Map(m => m.UserBarcode).Name("User barcode");
        Map(m => m.ItemBarcode).Name("Item barcode");
        Map(m => m.DateRaw).Name("Date");
        Map(m => m.Description).Name("Description");
    }
}
