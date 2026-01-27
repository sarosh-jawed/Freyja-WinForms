using CsvHelper.Configuration;
using Freyja.Models;

namespace Freyja.Csv.CsvMaps;

public sealed class ItemMatchedMap : ClassMap<ItemMatchedRow>
{
    public ItemMatchedMap()
    {
        Map(m => m.Barcode).Name("Barcode");
        Map(m => m.Instance).Name("Instance (Title, Publisher, Publication date)");
        Map(m => m.MaterialType).Name("Material type");
    }
}
