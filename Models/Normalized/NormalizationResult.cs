namespace Freyja.Models.Normalized;

public sealed class NormalizationResult
{
    public required Dictionary<string, NormalizedUser> UsersByExternalId { get; init; }
    public required Dictionary<string, NormalizedItem> ItemsByBarcode { get; init; }
    public required List<NormalizedCirculationEvent> EventsReadyForJoin { get; init; }
    public required List<NormalizationError> Errors { get; init; }
}
