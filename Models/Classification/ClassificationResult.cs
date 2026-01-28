namespace Freyja.Models.Classification;

public sealed class ClassificationResult
{
    public required List<ClassifiedLineItem> ForgivenLineItems { get; init; }
    public required List<ClassifiedLineItem> FineLineItems { get; init; }

    public required int ChargesProcessed { get; init; }
    public required int RecordsProcessed { get; init; }

    public required decimal ThresholdUsed { get; init; }
    public required List<string> SelectedPatronGroups { get; init; }
}
