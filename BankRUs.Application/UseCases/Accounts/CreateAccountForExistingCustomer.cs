using BankRUs.Application.Interfaces;
using BankRUs.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BankRUs.Application.UseCases.Accounts;

public record CreateAccountForExistingCustomerRequest(
    Guid CustomerId,
    decimal InitialBalance = 0m
);

public record CreateAccountForExistingCustomerResponse(
    Guid AccountId,
    string AccountNumber
);

public class CreateAccountForExistingCustomer
{
    private readonly ICustomerRepository _customers;
    private readonly IAccountRepository _accounts;
    private readonly IAccountNumberGenerator _accountNumberGenerator;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CreateAccountForExistingCustomer> _logger;

    public CreateAccountForExistingCustomer(
        ICustomerRepository customers,
        IAccountRepository accounts,
        IAccountNumberGenerator accountNumberGenerator,
        IUnitOfWork uow,
        ILogger<CreateAccountForExistingCustomer> logger)
    {
        _customers = customers;
        _accounts = accounts;
        _accountNumberGenerator = accountNumberGenerator;
        _uow = uow;
        _logger = logger;
    }

    public async Task<CreateAccountForExistingCustomerResponse> ExecuteAsync(
        CreateAccountForExistingCustomerRequest req,
        CancellationToken ct)
    {
        if (req.CustomerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required.", nameof(req.CustomerId));

        if (req.InitialBalance < 0m)
            throw new ArgumentOutOfRangeException(nameof(req.InitialBalance), "Initial balance cannot be negative.");

        var customer = await _customers.GetByIdAsync(req.CustomerId, ct);
        if (customer is null)
            throw new KeyNotFoundException("Customer not found.");

        var accountNumber = await _accountNumberGenerator.GenerateAsync(ct);

        if (await _accounts.AccountNumberExistsAsync(accountNumber, ct))
            throw new InvalidOperationException("Generated account number already exists. Try again.");

        var account = new BankAccount(customer.Id, accountNumber, req.InitialBalance);
        await _accounts.AddAsync(account, ct);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created additional account {AccountId} ({AccountNumber}) for existing customer {CustomerId} with initial balance {InitialBalance}",
            account.Id,
            account.AccountNumber,
            customer.Id,
            account.Balance);

        return new CreateAccountForExistingCustomerResponse(account.Id, account.AccountNumber);
    }
}