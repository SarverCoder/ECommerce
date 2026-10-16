using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Products;
using ECommerce.Api.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/products")]
public class ProductsController(IProductService service) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetPaged([FromQuery] ProductQuery query, CancellationToken ct)
        => Ok(await service.GetPagedAsync(query, ct));

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductRequest request, CancellationToken ct)
        => Ok(await service.UpdateAsync(id, request, ct));

    [HttpPost("{id:guid}/stock")]
    public async Task<ActionResult<ProductDto>> AddStock(Guid id, AdjustStockRequest request, CancellationToken ct)
        => Ok(await service.AddStockAsync(id, request.Quantity, ct));

    [HttpPut("{id:guid}/image")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ProductImageRules.MaxBytes + 1024 * 1024)] // file + multipart overhead
    public async Task<ActionResult<ProductDto>> UploadImage(Guid id, IFormFile? file, CancellationToken ct)
    {
        if (file is null)
            return Ok(await service.SetImageAsync(id, new UploadImageRequest(Stream.Null, string.Empty, 0), ct));

        await using var stream = file.OpenReadStream();
        return Ok(await service.SetImageAsync(id, new UploadImageRequest(stream, file.FileName, file.Length), ct));
    }

    [HttpDelete("{id:guid}/image")]
    public async Task<IActionResult> RemoveImage(Guid id, CancellationToken ct)
    {
        await service.RemoveImageAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await service.SetActiveAsync(id, true, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await service.SetActiveAsync(id, false, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
