namespace Freyja.Models.Output;

public sealed class OutputGenerationResult
{
    public required string ForgivenPath { get; init; }
    public required string FinePath { get; init; }
    public required string ErrorLogPath { get; init; }

    public int ForgivenLinesWritten { get; init; }
    public int FineLinesWritten { get; init; }
    public int ErrorLinesWritten { get; init; }
}
