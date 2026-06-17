using TechMoveLogisticsApplication.Models;

namespace TechMoveLogisticsApplication.Services.Api;

public record ClientOptionDto(int ClientId, string Name, string ContactDetails, string Region);

public record ClientDto(
    int ClientId,
    string Name,
    string ContactDetails,
    string Region,
    int ContractCount,
    IReadOnlyList<ContractDto> Contracts);

public record ContractDto(
    int ContractId,
    int ClientId,
    string? ClientName,
    ContractType ContractType,
    DateTime StartDate,
    DateTime EndDate,
    ContractStatus Status,
    string ServiceLevel,
    string? SignedAgreementFileName,
    DateTime CreatedAt,
    int ServiceRequestCount,
    string? CurrencyCode,
    string? ExchangeRule,
    int? PriorityLevel);

public record ServiceRequestDto(
    int ServiceRequestId,
    int ContractId,
    string? ClientName,
    ContractType ContractType,
    string RequestType,
    string Description,
    decimal RequestedAmountUsd,
    string CurrencyCode,
    decimal ExchangeRate,
    decimal Cost,
    ServiceRequestStatus Status,
    DateTime CreatedAt);

public record InvoiceDto(
    int InvoiceId,
    string? ClientName,
    string? RequestType,
    InvoiceStatus Status,
    decimal AmountZar,
    DateTime IssuedAt);

public record AuditLogDto(
    int AuditLogId,
    DateTime CreatedAt,
    string EventType,
    int? ContractId,
    int? ServiceRequestId,
    string Message);

public record DashboardDto(
    int ClientCount,
    int ActiveContractCount,
    int ServiceRequestCount,
    int InvoiceCount,
    IReadOnlyList<ContractDto> RecentContracts,
    IReadOnlyList<ServiceRequestDto> RecentRequests);

public record AuthTokenDto(
    string Token,
    string Scheme,
    DateTime ExpiresAtUtc,
    int UserId,
    string FullName,
    string Email,
    string Role);

public record AuthUserDto(int UserId, string FullName, string Email, string Role);

public class ApiClientResult<T>
{
    private ApiClientResult(bool succeeded, T? value, string? errorMessage)
    {
        Succeeded = succeeded;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }

    public static ApiClientResult<T> Success(T value)
    {
        return new ApiClientResult<T>(true, value, null);
    }

    public static ApiClientResult<T> Failure(string errorMessage)
    {
        return new ApiClientResult<T>(false, default, errorMessage);
    }
}
