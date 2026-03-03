using System.Security.Claims;
using BankRUs.Application.UseCases.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankRUs.Api.Controllers;

[ApiController]
[Route("api/me")]
public class MeController : ControllerBase
{
    [HttpDelete]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> DeleteMe(
        [FromServices] CloseCustomerAccount useCase,
        CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var customerId))
            return Forbid();

        var command = new CloseCustomerAccountCommand(customerId);
        await useCase.HandleAsync(command, ct);

        return NoContent();
    }
}

