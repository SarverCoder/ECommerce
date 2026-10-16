using ECommerce.Infrastructure.Storage;
using Microsoft.Extensions.FileProviders;

namespace ECommerce.Api.Storage;

public static class StaticFilesExtensions
{
    /// <summary>Serves uploaded files (e.g. product images) from the local storage folder.</summary>
    public static WebApplication UseUploadedFiles(this WebApplication app)
    {
        var storage = app.Services.GetRequiredService<LocalFileStorage>();

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(storage.RootPath),
            RequestPath = storage.RequestPath,
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.XContentTypeOptions = "nosniff";
                ctx.Context.Response.Headers.CacheControl = "public, max-age=604800"; // file names are unique
            }
        });

        return app;
    }
}
