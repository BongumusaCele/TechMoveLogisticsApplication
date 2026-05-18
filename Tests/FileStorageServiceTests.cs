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
        using var stream = new MemoryStream("%PDF- test agreement"u8.ToArray());
        var file = new FormFile(stream, 0, stream.Length, "file", "contract.pdf");

        var result = service.ValidateSignedAgreement(file);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateSignedAgreement_RejectsFakePdfContent()
    {
        var service = new FileStorageService(new TestWebHostEnvironment());
        using var stream = new MemoryStream("not a real pdf"u8.ToArray());
        var file = new FormFile(stream, 0, stream.Length, "file", "contract.pdf");

        var result = service.ValidateSignedAgreement(file);

        Assert.False(result.IsValid);
        Assert.Contains("valid PDF", result.ErrorMessage);
    }

    [Fact]
    public void GetSignedAgreementPath_RejectsTraversalFileNames()
    {
        var service = new FileStorageService(new TestWebHostEnvironment());

        var exception = Assert.Throws<InvalidOperationException>(() => service.GetSignedAgreementPath(@"..\secret.pdf"));

        Assert.Contains("Invalid agreement file name", exception.Message);
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
