using BankRUs.Application.Interfaces;
using BankRUs.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BankRUs.Application.UseCases.Accounts;

public record OpenBankAccountRequest(
    Guid UserId,
    decimal InitialBalance = 0m
);

public record OpenBankAccountResponse(
    Guid AccountId,
    string AccountNumber,
    decimal Balance
);

public class OpenBankAccount
{
    private readonly ICustomerRepository _customers;
    private readonly IAccountRepository _accounts;
    private readonly IAccountNumberGenerator _accountNumberGenerator;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<OpenBankAccount> _logger;

    public OpenBankAccount(
        ICustomerRepository customers,
        IAccountRepository accounts,
        IAccountNumberGenerator accountNumberGenerator,
        IUnitOfWork uow,
        ILogger<OpenBankAccount> logger)
    {
        _customers = customers;
        _accounts = accounts;
        _accountNumberGenerator = accountNumberGenerator;
        _uow = uow;
        _logger = logger;
    }

    public async Task<OpenBankAccountResponse> ExecuteAsync(
        OpenBankAccountRequest req,
        CancellationToken ct)
    {
        if (req.UserId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(req.UserId));

        if (req.InitialBalance < 0m)
            throw new ArgumentOutOfRangeException(nameof(req.InitialBalance), "Initial balance cannot be negative.");

        var customer = await _customers.GetByIdAsync(req.UserId, ct);
        if (customer is null)
            throw new KeyNotFoundException("Customer not found.");

        var accountNumber = await _accountNumberGenerator.GenerateAsync(ct);

        if (await _accounts.AccountNumberExistsAsync(accountNumber, ct))
            throw new InvalidOperationException("Generated account number already exists. Try again.");

        var account = new BankAccount(customer.Id, accountNumber, req.InitialBalance);
        await _accounts.AddAsync(account, ct);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Opened bank account {AccountId} ({AccountNumber}) for customer {CustomerId} with initial balance {InitialBalance}",
            account.Id,
            account.AccountNumber,
            customer.Id,
            account.Balance);

        return new OpenBankAccountResponse(account.Id, account.AccountNumber, account.Balance);
    }
}

