namespace Freyja.Models.Normalized;

public sealed class NormalizedUser
{
    public required string ExternalSystemId { get; init; }
    public required string PatronGroup { get; init; }
    public required string DisplayName { get; init; } // "First Last"
}
