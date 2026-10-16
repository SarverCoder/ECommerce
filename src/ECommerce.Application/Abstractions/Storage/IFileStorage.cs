namespace ECommerce.Application.Abstractions.Storage;

public interface IFileStorage
{
    /// <summary>Saves the content under a generated file name and returns its storage-relative path.</summary>
    Task<string> SaveAsync(Stream content, string folder, string extension, CancellationToken ct = default);

    /// <summary>Deletes the file if it exists. Never throws for missing files.</summary>
    Task DeleteAsync(string path, CancellationToken ct = default);

    /// <summary>Builds the public URL clients use to download the file.</summary>
    string GetUrl(string path);
}
