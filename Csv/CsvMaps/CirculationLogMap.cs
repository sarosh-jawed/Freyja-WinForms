using CsvHelper.Configuration;
using Freyja.Models;

namespace Freyja.Csv.CsvMaps;

public sealed class CirculationLogMap : ClassMap<CirculationLogRow>
{
    public CirculationLogMap()
    {
        Map(m => m.UserBarcode).Name("User barcode");
        Map(m => m.ItemBarcode).Name("Item barcode");
        Map(m => m.Object).Name("Object");
        Map(m => m.CircAction).Name("Circ action");
        Map(m => m.DateRaw).Name("Date");
        Map(m => m.ServicePoint).Name("Service point");
        Map(m => m.Source).Name("Source");
        Map(m => m.Description).Name("Description");
    }
}
