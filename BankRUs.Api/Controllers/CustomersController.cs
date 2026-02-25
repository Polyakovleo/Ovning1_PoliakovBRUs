using BankRUs.Application.UseCases.Accounts;
using BankRUs.Application.UseCases.Customers;
using Microsoft.AspNetCore.Mvc;

namespace BankRUs.Api.Controllers;

[ApiController]
[Route("customers")]
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

    public record CreateAccountBody(decimal InitialBalance = 0m);
}