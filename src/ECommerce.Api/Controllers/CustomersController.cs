using ECommerce.Application.Features.Customers;
using ECommerce.Application.Features.Orders;
using ECommerce.Api.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/customers")]
public class CustomersController(ICustomerService service, IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> GetAll(CancellationToken ct)
        => Ok(await service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    [HttpGet("{id:guid}/orders")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrders(Guid id, CancellationToken ct)
        => Ok(await orderService.GetByCustomerAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, UpdateCustomerRequest request, CancellationToken ct)
        => Ok(await service.UpdateAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
