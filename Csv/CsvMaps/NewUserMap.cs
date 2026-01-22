using CsvHelper.Configuration;
using Freyja.Models;

namespace Freyja.Csv.CsvMaps;

public sealed class NewUserMap : ClassMap<NewUserRow>
{
    public NewUserMap()
    {
        Map(m => m.PatronGroup).Name("groups.group");

        // If exports change later, we can add fallback names like:
        // Map(m => m.ExternalSystemId).Name("users.external_system_id", "users.externalSystemId");
        Map(m => m.ExternalSystemId).Name("users.external_system_id");

        Map(m => m.FirstName).Name("users.first_name");
        Map(m => m.LastName).Name("users.last_name");
        Map(m => m.UserBarcode).Name("users.barcode");
    }
}
