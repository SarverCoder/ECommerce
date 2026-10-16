using ECommerce.Api.Authentication;
using ECommerce.Application.Features.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public class OrdersController(IOrderService service) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetMine(CancellationToken ct)
        => Ok(await service.GetMineAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<ActionResult<OrderDto>> Pay(Guid id, CancellationToken ct)
        => Ok(await service.PayAsync(id, ct));

    [HttpPost("{id:guid}/ship")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<OrderDto>> Ship(Guid id, CancellationToken ct)
        => Ok(await service.ShipAsync(id, ct));

    [HttpPost("{id:guid}/deliver")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<OrderDto>> Deliver(Guid id, CancellationToken ct)
        => Ok(await service.DeliverAsync(id, ct));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id, CancellationToken ct)
        => Ok(await service.CancelAsync(id, ct));
}
