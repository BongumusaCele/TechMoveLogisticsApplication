using TechMoveLogisticsApplication.ViewModels;

namespace TechMoveLogisticsApplication.Services.Workflow;

public interface IServiceRequestWorkflowService
{
    Task<ServiceRequestCreationResult> CreateApprovedRequestAsync(
        ServiceRequestCreateViewModel viewModel,
        CancellationToken cancellationToken = default);
}
