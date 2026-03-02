using BankRUs.Application.UseCases.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace BankRUs.Api.Controllers;

[ApiController]
[Route("api/bank-accounts")]
public class BankAccountsController : ControllerBase
{
    [HttpPost]
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

