namespace Freyja.Models.Combined;

public sealed class JoinResult
{
    public List<CombinedRecord> Records { get; } = new();
    public List<JoinError> Errors { get; } = new();

    public int TotalEventsInput { get; init; }
    public int EventsJoined { get; set; }
    public int MissingUsers { get; set; }
    public int MissingItems { get; set; }
}
