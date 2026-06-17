using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TechMoveLogisticsApplication.Models;
using TechMoveLogisticsApplication.ViewModels;

namespace TechMoveLogisticsApplication.Services.Api;

public class TechMoveApiClient : ITechMoveApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public TechMoveApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        var apiKey = configuration["ApiSettings:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", apiKey);
        }
    }

    public async Task<ApiClientResult<AuthTokenDto>> LoginAsync(LoginViewModel viewModel)
    {
        return await SendAsync<AuthTokenDto>(
            () => _httpClient.PostAsJsonAsync("api/auth/token", new
            {
                viewModel.Email,
                viewModel.Password
            }, JsonOptions));
    }

    public async Task<ApiClientResult<AuthUserDto>> RegisterAsync(RegisterViewModel viewModel)
    {
        return await SendAsync<AuthUserDto>(
            () => _httpClient.PostAsJsonAsync("api/auth/register", new
            {
                viewModel.FullName,
                viewModel.Email,
                viewModel.Password
            }, JsonOptions));
    }

    public async Task<ApiClientResult<IReadOnlyList<Contract>>> GetContractsAsync(
        DateTime? startDate,
        DateTime? endDate,
        ContractStatus? status)
    {
        var query = new List<string>();
        if (startDate.HasValue)
        {
            query.Add($"startDate={Uri.EscapeDataString(startDate.Value.ToString("O"))}");
        }

        if (endDate.HasValue)
        {
            query.Add($"endDate={Uri.EscapeDataString(endDate.Value.ToString("O"))}");
        }

        if (status.HasValue)
        {
            query.Add($"status={(int)status.Value}");
        }

        var url = "api/contracts" + (query.Count == 0 ? string.Empty : $"?{string.Join("&", query)}");
        var result = await SendAsync<IReadOnlyList<ContractDto>>(() => _httpClient.GetAsync(url));
        if (!result.Succeeded || result.Value is null)
        {
            return ApiClientResult<IReadOnlyList<Contract>>.Failure(result.ErrorMessage ?? "Contracts could not be loaded.");
        }

        return ApiClientResult<IReadOnlyList<Contract>>.Success(result.Value.Select(ToContract).ToList());
    }

    public async Task<ApiClientResult<Contract>> GetContractAsync(int id)
    {
        var result = await SendAsync<ContractDto>(() => _httpClient.GetAsync($"api/contracts/{id}"));
        return result.Succeeded && result.Value is not null
            ? ApiClientResult<Contract>.Success(ToContract(result.Value))
            : ApiClientResult<Contract>.Failure(result.ErrorMessage ?? "Contract could not be loaded.");
    }

    public async Task<ApiClientResult<Contract>> CreateContractAsync(ContractCreateViewModel viewModel)
    {
        using var content = new MultipartFormDataContent();
        AddContractFormFields(content, viewModel.ClientId, viewModel.StartDate, viewModel.EndDate, viewModel.Status, viewModel.ServiceLevel);
        content.Add(new StringContent(((int)viewModel.ContractType).ToString()), nameof(viewModel.ContractType));
        AddFile(content, viewModel.SignedAgreement, nameof(viewModel.SignedAgreement));

        var result = await SendAsync<ContractDto>(() => _httpClient.PostAsync("api/contracts", content));
        return result.Succeeded && result.Value is not null
            ? ApiClientResult<Contract>.Success(ToContract(result.Value))
            : ApiClientResult<Contract>.Failure(result.ErrorMessage ?? "Contract could not be created.");
    }

    public async Task<ApiClientResult<Contract>> UpdateContractAsync(int id, ContractEditViewModel viewModel)
    {
        using var content = new MultipartFormDataContent();
        AddContractFormFields(content, viewModel.ClientId, viewModel.StartDate, viewModel.EndDate, viewModel.Status, viewModel.ServiceLevel);
        AddOptionalString(content, viewModel.CurrencyCode, nameof(viewModel.CurrencyCode));
        AddOptionalString(content, viewModel.ExchangeRule, nameof(viewModel.ExchangeRule));
        if (viewModel.PriorityLevel.HasValue)
        {
            content.Add(new StringContent(viewModel.PriorityLevel.Value.ToString()), nameof(viewModel.PriorityLevel));
        }

        AddFile(content, viewModel.SignedAgreement, nameof(viewModel.SignedAgreement));

        var result = await SendAsync<ContractDto>(() => _httpClient.PutAsync($"api/contracts/{id}", content));
        return result.Succeeded && result.Value is not null
            ? ApiClientResult<Contract>.Success(ToContract(result.Value))
            : ApiClientResult<Contract>.Failure(result.ErrorMessage ?? "Contract could not be updated.");
    }

    public async Task<ApiClientResult<Contract>> UpdateContractStatusAsync(int id, ContractStatus status)
    {
        var result = await SendAsync<ContractDto>(
            () => _httpClient.PatchAsJsonAsync($"api/contracts/{id}/status", new { status }, JsonOptions));

        return result.Succeeded && result.Value is not null
            ? ApiClientResult<Contract>.Success(ToContract(result.Value))
            : ApiClientResult<Contract>.Failure(result.ErrorMessage ?? "Contract status could not be updated.");
    }

    public async Task<ApiClientResult<IReadOnlyList<ClientOptionDto>>> GetClientsAsync()
    {
        return await SendAsync<IReadOnlyList<ClientOptionDto>>(() => _httpClient.GetAsync("api/clients"));
    }

    public async Task<ApiClientResult<IReadOnlyList<Client>>> GetClientListAsync()
    {
        var result = await SendAsync<IReadOnlyList<ClientDto>>(() => _httpClient.GetAsync("api/clients"));
        return result.Succeeded && result.Value is not null
            ? ApiClientResult<IReadOnlyList<Client>>.Success(result.Value.Select(ToClient).ToList())
            : ApiClientResult<IReadOnlyList<Client>>.Failure(result.ErrorMessage ?? "Clients could not be loaded.");
    }

    public async Task<ApiClientResult<Client>> GetClientAsync(int id)
    {
        var result = await SendAsync<ClientDto>(() => _httpClient.GetAsync($"api/clients/{id}"));
        return result.Succeeded && result.Value is not null
            ? ApiClientResult<Client>.Success(ToClient(result.Value))
            : ApiClientResult<Client>.Failure(result.ErrorMessage ?? "Client could not be loaded.");
    }

    public async Task<ApiClientResult<Client>> CreateClientAsync(Client client)
    {
        var result = await SendAsync<ClientDto>(
            () => _httpClient.PostAsJsonAsync("api/clients", new
            {
                client.Name,
                client.ContactDetails,
                client.Region
            }, JsonOptions));

        return result.Succeeded && result.Value is not null
            ? ApiClientResult<Client>.Success(ToClient(result.Value))
            : ApiClientResult<Client>.Failure(result.ErrorMessage ?? "Client could not be created.");
    }

    public async Task<ApiClientResult<IReadOnlyList<ServiceRequest>>> GetServiceRequestsAsync()
    {
        var result = await SendAsync<IReadOnlyList<ServiceRequestDto>>(() => _httpClient.GetAsync("api/service-requests"));
        return result.Succeeded && result.Value is not null
            ? ApiClientResult<IReadOnlyList<ServiceRequest>>.Success(result.Value.Select(ToServiceRequest).ToList())
            : ApiClientResult<IReadOnlyList<ServiceRequest>>.Failure(result.ErrorMessage ?? "Service requests could not be loaded.");
    }

    public async Task<ApiClientResult<ServiceRequest>> CreateServiceRequestAsync(ServiceRequestCreateViewModel viewModel)
    {
        var result = await SendAsync<ServiceRequestDto>(
            () => _httpClient.PostAsJsonAsync("api/service-requests", new
            {
                viewModel.ContractId,
                viewModel.RequestType,
                viewModel.Description,
                viewModel.RequestedAmountUsd
            }, JsonOptions));

        return result.Succeeded && result.Value is not null
            ? ApiClientResult<ServiceRequest>.Success(ToServiceRequest(result.Value))
            : ApiClientResult<ServiceRequest>.Failure(result.ErrorMessage ?? "Service request could not be created.");
    }

    public async Task<ApiClientResult<IReadOnlyList<Invoice>>> GetInvoicesAsync()
    {
        var result = await SendAsync<IReadOnlyList<InvoiceDto>>(() => _httpClient.GetAsync("api/invoices"));
        return result.Succeeded && result.Value is not null
            ? ApiClientResult<IReadOnlyList<Invoice>>.Success(result.Value.Select(ToInvoice).ToList())
            : ApiClientResult<IReadOnlyList<Invoice>>.Failure(result.ErrorMessage ?? "Invoices could not be loaded.");
    }

    public async Task<ApiClientResult<IReadOnlyList<AuditLog>>> GetAuditLogsAsync()
    {
        var result = await SendAsync<IReadOnlyList<AuditLogDto>>(() => _httpClient.GetAsync("api/audit"));
        return result.Succeeded && result.Value is not null
            ? ApiClientResult<IReadOnlyList<AuditLog>>.Success(result.Value.Select(ToAuditLog).ToList())
            : ApiClientResult<IReadOnlyList<AuditLog>>.Failure(result.ErrorMessage ?? "Audit logs could not be loaded.");
    }

    public async Task<ApiClientResult<DashboardViewModel>> GetDashboardAsync()
    {
        var result = await SendAsync<DashboardDto>(() => _httpClient.GetAsync("api/dashboard"));
        if (!result.Succeeded || result.Value is null)
        {
            return ApiClientResult<DashboardViewModel>.Failure(result.ErrorMessage ?? "Dashboard data could not be loaded.");
        }

        var dashboard = new DashboardViewModel
        {
            ClientCount = result.Value.ClientCount,
            ActiveContractCount = result.Value.ActiveContractCount,
            ServiceRequestCount = result.Value.ServiceRequestCount,
            InvoiceCount = result.Value.InvoiceCount,
            RecentContracts = result.Value.RecentContracts.Select(ToContract).ToList(),
            RecentRequests = result.Value.RecentRequests.Select(ToServiceRequest).ToList()
        };

        return ApiClientResult<DashboardViewModel>.Success(dashboard);
    }

    public async Task<ApiClientResult<ContractAgreementDownload>> DownloadAgreementAsync(int id)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"api/contracts/{id}/agreement");
            if (!response.IsSuccessStatusCode)
            {
                return ApiClientResult<ContractAgreementDownload>.Failure(await BuildErrorMessageAsync(response));
            }

            var content = await response.Content.ReadAsByteArrayAsync();
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? $"contract-{id}.pdf";
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/pdf";

            return ApiClientResult<ContractAgreementDownload>.Success(new ContractAgreementDownload(content, fileName, contentType));
        }
        catch (HttpRequestException)
        {
            return ApiClientResult<ContractAgreementDownload>.Failure("The API is not reachable. Start the TechMoveLogisticsAPI project and try again.");
        }
        catch (TaskCanceledException)
        {
            return ApiClientResult<ContractAgreementDownload>.Failure("The API request timed out. Try again shortly.");
        }
    }

    private static void AddContractFormFields(
        MultipartFormDataContent content,
        int? clientId,
        DateTime startDate,
        DateTime endDate,
        ContractStatus status,
        string serviceLevel)
    {
        if (clientId.HasValue)
        {
            content.Add(new StringContent(clientId.Value.ToString()), nameof(ContractCreateViewModel.ClientId));
        }

        content.Add(new StringContent(startDate.ToString("O")), nameof(ContractCreateViewModel.StartDate));
        content.Add(new StringContent(endDate.ToString("O")), nameof(ContractCreateViewModel.EndDate));
        content.Add(new StringContent(((int)status).ToString()), nameof(ContractCreateViewModel.Status));
        content.Add(new StringContent(serviceLevel ?? string.Empty), nameof(ContractCreateViewModel.ServiceLevel));
    }

    private static void AddOptionalString(MultipartFormDataContent content, string? value, string name)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            content.Add(new StringContent(value), name);
        }
    }

    private static void AddFile(MultipartFormDataContent content, IFormFile? file, string name)
    {
        if (file is null || file.Length == 0)
        {
            return;
        }

        var fileContent = new StreamContent(file.OpenReadStream());
        fileContent.Headers.ContentType = new(file.ContentType);
        content.Add(fileContent, name, file.FileName);
    }

    private async Task<ApiClientResult<T>> SendAsync<T>(Func<Task<HttpResponseMessage>> request)
    {
        try
        {
            using var response = await request();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return ApiClientResult<T>.Failure("The requested API resource was not found.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return ApiClientResult<T>.Failure(await BuildErrorMessageAsync(response));
            }

            var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            return value is null
                ? ApiClientResult<T>.Failure("The API returned an empty response.")
                : ApiClientResult<T>.Success(value);
        }
        catch (HttpRequestException)
        {
            return ApiClientResult<T>.Failure("The API is not reachable. Start the TechMoveLogisticsAPI project and try again.");
        }
        catch (TaskCanceledException)
        {
            return ApiClientResult<T>.Failure("The API request timed out. Try again shortly.");
        }
    }

    private static async Task<string> BuildErrorMessageAsync(HttpResponseMessage response)
    {
        if (response.Content.Headers.ContentType?.MediaType == "application/problem+json")
        {
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            if (problem.TryGetProperty("title", out var title))
            {
                return title.GetString() ?? $"The API returned {(int)response.StatusCode}.";
            }
        }

        var body = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(body)
            ? $"The API returned {(int)response.StatusCode} {response.ReasonPhrase}."
            : body;
    }

    private static Contract ToContract(ContractDto dto)
    {
        Contract contract = dto.ContractType switch
        {
            ContractType.International => new InternationalContract
            {
                CurrencyCode = dto.CurrencyCode ?? "USD",
                ExchangeRule = dto.ExchangeRule ?? "Use external exchange API and store local ZAR cost"
            },
            ContractType.Premium => new PremiumContract
            {
                PriorityLevel = dto.PriorityLevel ?? 1
            },
            _ => new StandardContract()
        };

        contract.ContractId = dto.ContractId;
        contract.ClientId = dto.ClientId;
        contract.Client = new Client
        {
            ClientId = dto.ClientId,
            Name = dto.ClientName ?? "Unknown client"
        };
        contract.StartDate = dto.StartDate;
        contract.EndDate = dto.EndDate;
        contract.Status = dto.Status;
        contract.ServiceLevel = dto.ServiceLevel;
        contract.SignedAgreementFileName = dto.SignedAgreementFileName;
        contract.CreatedAt = dto.CreatedAt;
        contract.ServiceRequests = Enumerable
            .Range(0, dto.ServiceRequestCount)
            .Select(_ => new ServiceRequest())
            .ToList();

        return contract;
    }

    private static Client ToClient(ClientDto dto)
    {
        return new Client
        {
            ClientId = dto.ClientId,
            Name = dto.Name,
            ContactDetails = dto.ContactDetails,
            Region = dto.Region,
            Contracts = dto.Contracts.Select(ToContract).ToList()
        };
    }

    private static ServiceRequest ToServiceRequest(ServiceRequestDto dto)
    {
        return new ServiceRequest
        {
            ServiceRequestId = dto.ServiceRequestId,
            ContractId = dto.ContractId,
            Contract = CreateContractShell(dto.ContractType, dto.ContractId, dto.ClientName),
            RequestType = dto.RequestType,
            Description = dto.Description,
            RequestedAmountUsd = dto.RequestedAmountUsd,
            CurrencyCode = dto.CurrencyCode,
            ExchangeRate = dto.ExchangeRate,
            Cost = dto.Cost,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt
        };
    }

    private static Invoice ToInvoice(InvoiceDto dto)
    {
        return new Invoice
        {
            InvoiceId = dto.InvoiceId,
            ServiceRequest = new ServiceRequest
            {
                RequestType = dto.RequestType ?? "Service request",
                Contract = CreateContractShell(ContractType.Standard, 0, dto.ClientName)
            },
            Status = dto.Status,
            AmountZar = dto.AmountZar,
            IssuedAt = dto.IssuedAt
        };
    }

    private static AuditLog ToAuditLog(AuditLogDto dto)
    {
        return new AuditLog
        {
            AuditLogId = dto.AuditLogId,
            CreatedAt = dto.CreatedAt,
            EventType = dto.EventType,
            ContractId = dto.ContractId,
            ServiceRequestId = dto.ServiceRequestId,
            Message = dto.Message
        };
    }

    private static Contract CreateContractShell(ContractType contractType, int contractId, string? clientName)
    {
        Contract contract = contractType switch
        {
            ContractType.International => new InternationalContract(),
            ContractType.Premium => new PremiumContract(),
            _ => new StandardContract()
        };

        contract.ContractId = contractId;
        contract.Client = new Client { Name = clientName ?? "Unknown client" };
        contract.ServiceLevel = contractType.ToString();
        return contract;
    }
}
