using Microsoft.AspNetCore.Components.Forms;

namespace PortfolioApp.Web.Services;

public enum UploadKind { Image, Pdf }

public class UploadRejectedException : Exception
{
    public UploadRejectedException(string message) : base(message) { }
}

/// <summary>
/// Stores admin uploads under the uploads root (wwwroot/uploads by default, or Storage:UploadsPath),
/// served at /uploads. File types are verified from the file's signature bytes on the server and the
/// stored extension comes from the detected type, never from the client-supplied file name.
/// </summary>
public class FileStorageService
{
    public const string RequestPath = "/uploads";

    private readonly string _root;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IConfiguration config, IWebHostEnvironment env, ILogger<FileStorageService> logger)
    {
        _root = ResolveRoot(config, env);
        _logger = logger;
        Directory.CreateDirectory(_root);
    }

    public static string ResolveRoot(IConfiguration config, IWebHostEnvironment env)
    {
        var configured = config["Storage:UploadsPath"];
        if (string.IsNullOrWhiteSpace(configured))
            return Path.Combine(env.WebRootPath, "uploads");
        return Path.GetFullPath(Path.IsPathRooted(configured) ? configured : Path.Combine(env.ContentRootPath, configured));
    }

    /// <summary>Saves the file and returns its public URL, e.g. /uploads/projects/abc.png.</summary>
    public async Task<string> SaveAsync(IBrowserFile file, string folder, UploadKind kind, long maxBytes, CancellationToken ct = default)
    {
        if (file.Size > maxBytes)
            throw new UploadRejectedException($"File is too large (max {maxBytes / (1024 * 1024)} MB).");

        using var buffer = new MemoryStream();
        await using (var input = file.OpenReadStream(maxBytes, ct))
            await input.CopyToAsync(buffer, ct);

        var extension = DetectExtension(new ReadOnlySpan<byte>(buffer.GetBuffer(), 0, (int)buffer.Length), kind)
            ?? throw new UploadRejectedException(kind == UploadKind.Pdf
                ? "Only PDF files are allowed."
                : "Only JPG, PNG or WebP images are allowed.");

        var safeFolder = string.Concat(folder.Where(char.IsLetterOrDigit));
        var dir = Path.Combine(_root, safeFolder);
        Directory.CreateDirectory(dir);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        buffer.Position = 0;
        await using (var output = new FileStream(Path.Combine(dir, fileName), FileMode.CreateNew))
            await buffer.CopyToAsync(output, ct);

        return $"{RequestPath}/{safeFolder}/{fileName}";
    }

    /// <summary>Deletes a previously uploaded file. URLs outside /uploads (e.g. bundled images) are ignored.</summary>
    public void Delete(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !url.StartsWith(RequestPath + "/", StringComparison.OrdinalIgnoreCase))
            return;

        var relative = url[(RequestPath.Length + 1)..].Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_root, relative));
        if (!fullPath.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return; // path traversal attempt

        try
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Could not delete uploaded file {Path}", fullPath);
        }
    }

    internal static string? DetectExtension(ReadOnlySpan<byte> b, UploadKind kind)
    {
        if (kind == UploadKind.Pdf)
            return b.Length >= 5 && b[..5].SequenceEqual("%PDF-"u8) ? ".pdf" : null;

        if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF) return ".jpg";
        if (b.Length >= 8 && b[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return ".png";
        if (b.Length >= 12 && b[..4].SequenceEqual("RIFF"u8) && b[8..12].SequenceEqual("WEBP"u8)) return ".webp";
        return null;
    }
}

/// <summary>
/// Tracks uploads made while an edit form is open so files are cleaned up whether the edit is saved
/// (the replaced original and any superseded uploads are deleted) or abandoned (new uploads are deleted).
/// </summary>
public sealed class UploadTracker
{
    private readonly FileStorageService _storage;
    private readonly List<string> _pending = new();
    private string? _original;

    public UploadTracker(FileStorageService storage) => _storage = storage;

    public void Begin(string? originalUrl)
    {
        Rollback();
        _original = originalUrl;
    }

    public void Track(string url) => _pending.Add(url);

    /// <summary>Call after the record has been saved with <paramref name="savedUrl"/>.</summary>
    public void Commit(string? savedUrl)
    {
        foreach (var url in _pending.Where(u => u != savedUrl)) _storage.Delete(url);
        _pending.Clear();
        if (_original != savedUrl) _storage.Delete(_original);
        _original = savedUrl;
    }

    /// <summary>Call when the edit is cancelled; deletes files uploaded during this edit.</summary>
    public void Rollback()
    {
        foreach (var url in _pending) _storage.Delete(url);
        _pending.Clear();
    }
}
