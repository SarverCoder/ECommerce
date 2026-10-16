using ECommerce.Application.Abstractions.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly LocalFileStorageOptions _options;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(IOptions<LocalFileStorageOptions> options, IHostEnvironment env, ILogger<LocalFileStorage> logger)
    {
        _options = options.Value;
        _logger = logger;
        RootPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, _options.RootPath));
        Directory.CreateDirectory(RootPath);
    }

    /// <summary>Absolute folder the files live in (used by the API to serve them).</summary>
    public string RootPath { get; }

    public string RequestPath => _options.RequestPath;

    public async Task<string> SaveAsync(Stream content, string folder, string extension, CancellationToken ct = default)
    {
        // Never trust the client's file name: generate our own
        var relativePath = $"{folder}/{Guid.CreateVersion7():N}{extension}";
        var fullPath = Resolve(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        if (content.CanSeek)
            content.Position = 0;

        await using var file = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write,
            FileShare.None, bufferSize: 81920, useAsync: true);
        await content.CopyToAsync(file, ct);

        return relativePath;
    }

    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        try
        {
            var fullPath = Resolve(path);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // A leftover file is not worth failing the request for
            _logger.LogWarning(ex, "Could not delete file {Path}", path);
        }
        return Task.CompletedTask;
    }

    public string GetUrl(string path) => $"{_options.RequestPath.TrimEnd('/')}/{path}";

    private string Resolve(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(RootPath, relativePath));
        if (!fullPath.StartsWith(RootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Path escapes the storage root.");
        return fullPath;
    }
}
