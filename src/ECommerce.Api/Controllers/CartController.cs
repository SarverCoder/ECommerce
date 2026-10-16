using ECommerce.Application.Features.Carts;
using ECommerce.Application.Features.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/cart")]
public class CartController(ICartService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CartDto>> Get(CancellationToken ct)
        => Ok(await service.GetMineAsync(ct));

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem(AddCartItemRequest request, CancellationToken ct)
        => Ok(await service.AddItemAsync(request, ct));

    [HttpPut("items/{productId:guid}")]
    public async Task<ActionResult<CartDto>> UpdateItem(Guid productId, UpdateCartItemRequest request, CancellationToken ct)
        => Ok(await service.UpdateItemAsync(productId, request, ct));

    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult<CartDto>> RemoveItem(Guid productId, CancellationToken ct)
        => Ok(await service.RemoveItemAsync(productId, ct));

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        await service.ClearAsync(ct);
        return NoContent();
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<OrderDto>> Checkout(CheckoutRequest request, CancellationToken ct)
    {
        var order = await service.CheckoutAsync(request, ct);
        return CreatedAtAction(nameof(OrdersController.GetById), "Orders", new { id = order.Id }, order);
    }
}
