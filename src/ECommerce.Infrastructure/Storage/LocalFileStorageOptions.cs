namespace ECommerce.Infrastructure.Storage;

public class LocalFileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>Folder for uploaded files; relative paths are resolved against the content root.</summary>
    public string RootPath { get; init; } = "uploads";

    /// <summary>URL prefix the files are served under.</summary>
    public string RequestPath { get; init; } = "/uploads";
}
