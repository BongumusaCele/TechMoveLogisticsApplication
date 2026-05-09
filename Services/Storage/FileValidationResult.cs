namespace TechMoveLogisticsApplication.Services.Storage;

public class FileValidationResult
{
    public bool IsValid => string.IsNullOrEmpty(ErrorMessage);
    public string? ErrorMessage { get; init; }

    public static FileValidationResult Success() => new();

    public static FileValidationResult Failure(string message) => new()
    {
        ErrorMessage = message
    };
}
