using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using TechMoveLogisticsApplication.Services.Storage;

namespace TechMoveLogisticsApplication.Tests;

public class FileStorageServiceTests
{
    [Fact]
    public void ValidateSignedAgreement_RejectsRestrictedFileType()
    {
        var service = new FileStorageService(new TestWebHostEnvironment());
        using var stream = new MemoryStream([1, 2, 3]);
        var file = new FormFile(stream, 0, stream.Length, "file", "contract.exe");

        var result = service.ValidateSignedAgreement(file);

        Assert.False(result.IsValid);
        Assert.Contains("Only PDF", result.ErrorMessage);
    }

    [Fact]
    public void ValidateSignedAgreement_AllowsPdfFile()
    {
        var service = new FileStorageService(new TestWebHostEnvironment());
        using var stream = new MemoryStream([1, 2, 3]);
        var file = new FormFile(stream, 0, stream.Length, "file", "contract.pdf");

        var result = service.ValidateSignedAgreement(file);

        Assert.True(result.IsValid);
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public string EnvironmentName { get; set; } = "Development";
        public string WebRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
