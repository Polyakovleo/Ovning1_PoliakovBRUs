using BankRUs.Application.UseCases.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace BankRUs.Api.Controllers;

[ApiController]
[Route("api/bank-accounts")]
public class BankAccountsController : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Open bank account for customer")]
    [EndpointDescription("Opens a new bank account for an existing customer specified by userId, with an optional initial balance.")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(OpenBankAccountResponse), StatusCodes.Status200OK)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public async Task<ActionResult<OpenBankAccountResponse>> Open(
        [FromServices] OpenBankAccount useCase,
        [FromBody] OpenBankAccountBody body,
        CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(
            new OpenBankAccountRequest(body.UserId, body.InitialBalance),
            ct);

        return Ok(result);
    }

    public record OpenBankAccountBody(
        Guid UserId,
        decimal InitialBalance = 0m
    );
}

