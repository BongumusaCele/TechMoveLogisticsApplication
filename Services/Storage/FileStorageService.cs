using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace TechMoveLogisticsApplication.Services.Storage;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private const long MaxFileSize = 5 * 1024 * 1024;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public FileValidationResult ValidateSignedAgreement(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return FileValidationResult.Failure("A signed PDF agreement is required.");
        }

        if (file.Length > MaxFileSize)
        {
            return FileValidationResult.Failure("Signed agreements must be smaller than 5 MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return FileValidationResult.Failure("Only PDF signed agreements are allowed.");
        }

        return FileValidationResult.Success();
    }

    public async Task<string?> SaveContractAgreementAsync(IFormFile? file, int contractId)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        var validation = ValidateSignedAgreement(file);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.ErrorMessage);
        }

        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "contracts");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"contract-{contractId}-{Guid.NewGuid():N}.pdf";
        var fullPath = Path.Combine(uploadsPath, fileName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream);

        return fileName;
    }

    public string GetSignedAgreementPath(string fileName)
    {
        return Path.Combine(_environment.WebRootPath, "uploads", "contracts", fileName);
    }
}
