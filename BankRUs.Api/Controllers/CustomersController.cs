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
    [EndpointSummary("Create customer with first bank account")]
    [EndpointDescription("Creates a new customer together with an initial bank account. Returns 201 with customer and account identifiers.")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CreateCustomerWithAccountResponse), StatusCodes.Status201Created)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<ActionResult<CreateCustomerWithAccountResponse>> Create(
        [FromServices] CreateCustomerWithAccount useCase,
        [FromBody] CreateCustomerWithAccountRequest request,
        CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(request, ct);
        return Created($"/customers/{result.CustomerId}", result);
    }

    [HttpPost("{customerId:guid}/accounts")]
    [EndpointSummary("Create additional bank account for existing customer")]
    [EndpointDescription("Creates an additional bank account for an existing customer identified by route parameter customerId.")]
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
    [EndpointSummary("Get paged list of customers")]
    [EndpointDescription("Returns a paginated list of customers for CustomerService role. Supports page, pageSize and ssn (personal number prefix) filters.")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PagedResult<CustomerListItemDto>), StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
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
    [EndpointSummary("Get single customer by id")]
    [EndpointDescription("Returns detailed information about a single customer, including all bank accounts, for CustomerService role.")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CustomerDetailDto), StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
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
    [EndpointSummary("Update current customer's profile")]
    [EndpointDescription("Allows an authenticated customer to update their name, email and/or personal number based on the authenticated user id.")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
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