namespace Freyja.Models.Normalized;

public enum NormalizationErrorType
{
    InvalidUserBarcode,
    MissingUser,
    MissingItem,
    BadDescriptionParse,
    BadAmountParse
}

public sealed class NormalizationError
{
    public required NormalizationErrorType Type { get; init; }
    public string? UserBarcode { get; init; }
    public string? ItemBarcode { get; init; }
    public required string Message { get; init; }
}
