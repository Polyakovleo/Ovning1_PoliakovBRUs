using BankRUs.Application.UseCases.Accounts;
using BankRUs.Application.UseCases.Customers;
using Microsoft.AspNetCore.Mvc;
using BankRUs.Application.Common.Paging;
using BankRUs.Api;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BankRUs.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateCustomerWithAccountResponse>> Create(
        [FromServices] CreateCustomerWithAccount useCase,
        [FromBody] CreateCustomerWithAccountRequest request,
        CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(request, ct);
        return Created($"/customers/{result.CustomerId}", result);
    }

    [HttpPost("{customerId:guid}/accounts")]
    public async Task<ActionResult<CreateAccountForExistingCustomerResponse>> CreateAccount(
        Guid customerId,
        [FromServices] CreateAccountForExistingCustomer useCase,
        [FromBody] CreateAccountBody body,
        CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(
            new CreateAccountForExistingCustomerRequest(customerId, body.InitialBalance),
            ct);
    

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "CustomerService")]
    public async Task<ActionResult<PagedResult<CustomerListItemDto>>> GetPage(
        [FromServices] GetCustomersPage useCase,
        [FromServices] IOptions<QueryParamsOptions> options,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery(Name = "ssn")] string? ssn = null,
        CancellationToken ct = default)
    {
        var maxPageSize = options.Value.MaxPageSize;

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > maxPageSize) pageSize = maxPageSize;

        var result = await useCase.ExecuteAsync(
            new GetCustomersPageQuery(page, pageSize, ssn),
            ct);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "CustomerService")]
    public async Task<ActionResult<CustomerDetailDto>> GetById(
        Guid id,
        [FromServices] GetCustomerById useCase,
        CancellationToken ct = default)
    {
        var result = await useCase.ExecuteAsync(id, ct);
        return Ok(result);
    }

    [HttpPatch]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UpdateMe(
        [FromServices] UpdateCustomerDetails useCase,
        [FromBody] UpdateCustomerDetailsBody body,
        CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var customerId))
            return Forbid();

        var command = new UpdateCustomerDetailsCommand(
            customerId,
            body.FirstName,
            body.Email,
            body.PersonalNumber);

        await useCase.HandleAsync(command, ct);

        return NoContent();
    }

    public record CreateAccountBody(decimal InitialBalance = 0m);

    public record UpdateCustomerDetailsBody(
        string? FirstName,
        string? Email,
        string? PersonalNumber);
}