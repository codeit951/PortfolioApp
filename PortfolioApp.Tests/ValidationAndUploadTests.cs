using System.ComponentModel.DataAnnotations;
using System.Text;
using PortfolioApp.Domain.Entities;
using PortfolioApp.Web.Services;

namespace PortfolioApp.Tests;

public class ValidationAndUploadTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Profile_allows_empty_optional_links_and_email()
    {
        Assert.Empty(Validate(new UserProfile { FullName = "Name" }));
    }

    [Fact]
    public void Profile_rejects_invalid_links()
    {
        var results = Validate(new UserProfile { FullName = "Name", GitHubUrl = "javascript:alert(1)", Email = "nope" });
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UserProfile.GitHubUrl)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UserProfile.Email)));
    }

    [Fact]
    public void Project_image_is_optional()
    {
        Assert.Empty(Validate(new Project { Title = "T", Description = "D" }));
    }

    [Fact]
    public void Past_experience_needs_valid_end_date()
    {
        var missing = new Experience { Company = "C", Role = "R", StartDate = new DateTime(2020, 1, 1) };
        Assert.Contains(Validate(missing), r => r.MemberNames.Contains(nameof(Experience.EndDate)));

        var backwards = new Experience { Company = "C", Role = "R", StartDate = new DateTime(2020, 1, 1), EndDate = new DateTime(2019, 1, 1) };
        Assert.Contains(Validate(backwards), r => r.MemberNames.Contains(nameof(Experience.EndDate)));

        var current = new Experience { Company = "C", Role = "R", StartDate = new DateTime(2020, 1, 1), IsCurrent = true };
        Assert.Empty(Validate(current));
    }

    [Theory]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, ".jpg")]
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 }, ".png")]
    public void Detects_real_images(byte[] header, string expected)
    {
        Assert.Equal(expected, FileStorageService.DetectExtension(header, UploadKind.Image));
    }

    [Fact]
    public void Detects_webp_and_pdf()
    {
        var webp = Encoding.ASCII.GetBytes("RIFF\0\0\0\0WEBPVP8 ");
        Assert.Equal(".webp", FileStorageService.DetectExtension(webp, UploadKind.Image));
        Assert.Equal(".pdf", FileStorageService.DetectExtension(Encoding.ASCII.GetBytes("%PDF-1.7"), UploadKind.Pdf));
    }

    [Theory]
    [InlineData("<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>")]
    [InlineData("<html><script>alert(1)</script></html>")]
    [InlineData("")]
    public void Rejects_scriptable_or_unknown_files(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        Assert.Null(FileStorageService.DetectExtension(bytes, UploadKind.Image));
        Assert.Null(FileStorageService.DetectExtension(bytes, UploadKind.Pdf));
    }

    [Fact]
    public void Pdf_upload_rejects_images()
    {
        Assert.Null(FileStorageService.DetectExtension(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, UploadKind.Pdf));
    }
}
