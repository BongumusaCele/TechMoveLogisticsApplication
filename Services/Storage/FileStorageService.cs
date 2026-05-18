using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace TechMoveLogisticsApplication.Services.Storage;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private const long MaxFileSize = 5 * 1024 * 1024;
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();

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

        if (!HasPdfSignature(file))
        {
            return FileValidationResult.Failure("The uploaded agreement must be a valid PDF file.");
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
        var safeFileName = Path.GetFileName(fileName);
        if (!string.Equals(safeFileName, fileName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Invalid agreement file name.");
        }

        var uploadsPath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "uploads", "contracts"));
        var fullPath = Path.GetFullPath(Path.Combine(uploadsPath, safeFileName));

        if (!fullPath.StartsWith(uploadsPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid agreement file path.");
        }

        return fullPath;
    }

    private static bool HasPdfSignature(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[PdfSignature.Length];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);

            return bytesRead == PdfSignature.Length && buffer.SequenceEqual(PdfSignature);
        }
        catch
        {
            return false;
        }
    }
}
