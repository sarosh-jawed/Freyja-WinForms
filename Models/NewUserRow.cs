namespace Freyja.Models;

public sealed class NewUserRow
{
    public string PatronGroup { get; set; } = "";       // groups.group
    public string ExternalSystemId { get; set; } = "";  // users.external_system_id
    public string FirstName { get; set; } = "";         // users.first_name
    public string LastName { get; set; } = "";          // users.last_name
    public string UserBarcode { get; set; } = "";       // users.barcode 
}
