namespace Freyja.Models.Combined;

public sealed class CombinedRecord
{
    // Join keys
    public string UserExternalSystemId { get; init; } = "";
    public string ItemBarcode { get; init; } = "";

    // User fields
    public string UserFullName { get; init; } = "";
    public string PatronGroup { get; init; } = "";  // groups.group from Users CSV

    // Item fields
    public string InstanceSummary { get; init; } = ""; // "Instance (Title, Publisher, Publication date)"
    public string MaterialType { get; init; } = "";

    // All circulation charges for this user+item
    public List<CombinedCharge> Charges { get; } = new();
}
