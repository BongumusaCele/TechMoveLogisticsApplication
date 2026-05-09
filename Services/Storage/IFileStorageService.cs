using Microsoft.AspNetCore.Http;

namespace TechMoveLogisticsApplication.Services.Storage;

public interface IFileStorageService
{
    FileValidationResult ValidateSignedAgreement(IFormFile? file);
    Task<string?> SaveContractAgreementAsync(IFormFile? file, int contractId);
    string GetSignedAgreementPath(string fileName);
}
