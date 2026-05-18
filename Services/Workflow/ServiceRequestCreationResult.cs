namespace TechMoveLogisticsApplication.Services.Workflow;

public sealed class ServiceRequestCreationResult
{
    public int? ServiceRequestId { get; private set; }
    public List<ServiceRequestCreationError> Errors { get; } = new();
    public bool Succeeded => ServiceRequestId.HasValue && Errors.Count == 0;

    public void AddError(string? fieldName, string message)
    {
        Errors.Add(new ServiceRequestCreationError(fieldName, message));
    }

    public void MarkCreated(int serviceRequestId)
    {
        ServiceRequestId = serviceRequestId;
    }
}

public sealed record ServiceRequestCreationError(string? FieldName, string Message);
