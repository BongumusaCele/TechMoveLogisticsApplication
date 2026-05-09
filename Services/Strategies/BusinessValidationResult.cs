namespace TechMoveLogisticsApplication.Services.Strategies;

public class BusinessValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; } = new();

    public void AddError(string message)
    {
        Errors.Add(message);
    }
}
