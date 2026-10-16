using ECommerce.Api.Authentication;
using ECommerce.Api.Filters;
using ECommerce.Api.Middleware;
using ECommerce.Api.OpenApi;
using ECommerce.Api.Storage;
using ECommerce.Application;
using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
    // FluentValidation is the single source of truth; don't add implicit [Required] for non-nullable strings
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                    // /openapi/v1.json
    app.UseSwaggerUI(options =>          // UI: /swagger
    {
        options.SwaggerEndpoint("/openapi/v1.json", "ECommerce API v1");
        options.DocumentTitle = "ECommerce API";
        options.EnablePersistAuthorization();
    });
    app.MapScalarApiReference();         // UI: /scalar

    await DbSeeder.SeedAdminAsync(app.Services);
}

app.UseHttpsRedirection();
app.UseUploadedFiles();              // /uploads/products/{file}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
