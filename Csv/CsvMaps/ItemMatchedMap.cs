using CsvHelper.Configuration;
using Freyja.Models;

namespace Freyja.Csv.CsvMaps;

public sealed class ItemMatchedMap : ClassMap<ItemMatchedRow>
{
    public ItemMatchedMap()
    {
        Map(m => m.Barcode).Name("Barcode");
        Map(m => m.InstanceSummary).Name("Instance (Title, Publisher, Publication date)");
    }
}
