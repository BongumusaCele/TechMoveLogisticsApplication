using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.ViewModels;

namespace TechMoveLogisticsApplication.Services.Api;

public interface ITechMoveApiClient
{
    Task<ApiClientResult<AuthTokenDto>> LoginAsync(LoginViewModel viewModel);
    Task<ApiClientResult<AuthUserDto>> RegisterAsync(RegisterViewModel viewModel);
    Task<ApiClientResult<IReadOnlyList<Contract>>> GetContractsAsync(DateTime? startDate, DateTime? endDate, ContractStatus? status);
    Task<ApiClientResult<Contract>> GetContractAsync(int id);
    Task<ApiClientResult<Contract>> CreateContractAsync(ContractCreateViewModel viewModel);
    Task<ApiClientResult<Contract>> UpdateContractAsync(int id, ContractEditViewModel viewModel);
    Task<ApiClientResult<Contract>> UpdateContractStatusAsync(int id, ContractStatus status);
    Task<ApiClientResult<IReadOnlyList<ClientOptionDto>>> GetClientsAsync();
    Task<ApiClientResult<IReadOnlyList<Client>>> GetClientListAsync();
    Task<ApiClientResult<Client>> GetClientAsync(int id);
    Task<ApiClientResult<Client>> CreateClientAsync(Client client);
    Task<ApiClientResult<IReadOnlyList<ServiceRequest>>> GetServiceRequestsAsync();
    Task<ApiClientResult<ServiceRequest>> CreateServiceRequestAsync(ServiceRequestCreateViewModel viewModel);
    Task<ApiClientResult<IReadOnlyList<Invoice>>> GetInvoicesAsync();
    Task<ApiClientResult<IReadOnlyList<AuditLog>>> GetAuditLogsAsync();
    Task<ApiClientResult<DashboardViewModel>> GetDashboardAsync();
    Task<ApiClientResult<ContractAgreementDownload>> DownloadAgreementAsync(int id);
}

public record ContractAgreementDownload(byte[] Content, string FileName, string ContentType);
