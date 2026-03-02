using BankRUs.Application.Common.Exceptions;
using BankRUs.Application.Interfaces;
using BankRUs.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BankRUs.Application.UseCases.Customers;

public record CreateCustomerWithAccountRequest(
    string Name,
    string Email,
    string PersonalNumber,
    decimal InitialBalance = 0m
);

public record CreateCustomerWithAccountResponse(
    Guid CustomerId,
    Guid AccountId,
    string AccountNumber
);

public class CreateCustomerWithAccount
{
    private readonly ICustomerRepository _customers;
    private readonly IAccountRepository _accounts;
    private readonly IAccountNumberGenerator _accountNumberGenerator;
    private readonly IUnitOfWork _uow;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<CreateCustomerWithAccount> _logger;

    public CreateCustomerWithAccount(
        ICustomerRepository customers,
        IAccountRepository accounts,
        IAccountNumberGenerator accountNumberGenerator,
        IUnitOfWork uow,
        IEmailSender emailSender,
        ILogger<CreateCustomerWithAccount> logger)
    {
        _customers = customers;
        _accounts = accounts;
        _accountNumberGenerator = accountNumberGenerator;
        _uow = uow;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<CreateCustomerWithAccountResponse> ExecuteAsync(
        CreateCustomerWithAccountRequest req,
        CancellationToken ct)
    {
        if (req.InitialBalance < 0m)
            throw new ArgumentOutOfRangeException(nameof(req.InitialBalance), "Initial balance cannot be negative.");

        // Уникальность (минимально)
        var existingByPn = await _customers.GetByPersonalNumberAsync(req.PersonalNumber, ct);
        if (existingByPn is not null)
            throw new DomainValidationException("Customer with this personal number already exists.");

        var existingByEmail = await _customers.GetByEmailAsync(req.Email, ct);
        if (existingByEmail is not null)
            throw new DomainValidationException("Customer with this email already exists.");

        // Создаем Customer
        var customer = new Customer(req.Name, req.Email, req.PersonalNumber);
        await _customers.AddAsync(customer, ct);

        // Генерим номер аккаунта + создаём BankAccount, привязанный к customer.Id
        var accountNumber = await _accountNumberGenerator.GenerateAsync(ct);

        // (опционально) если генератор не гарантирует уникальность:
        if (await _accounts.AccountNumberExistsAsync(accountNumber, ct))
            throw new DomainValidationException("Generated account number already exists. Try again.");

        var account = new BankAccount(customer.Id, accountNumber, req.InitialBalance);
        await _accounts.AddAsync(account, ct);

        // Один коммит на всё
        await _uow.SaveChangesAsync(ct);

        // Пытаемся отправить приветственное письмо, но не ломаем основной флоу
        try
        {
            var subject = "Välkommen till BankRUs!";
            var body = $@"
<h2>Hej {customer.Name}!</h2>
<p>Ditt bankkonto är skapat.</p>
<p><b>Kontonummer:</b> {account.AccountNumber}</p>
<p>Saldo: {account.Balance} SEK</p>";

            await _emailSender.SendAsync(customer.Email, subject, body, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send welcome email for newly created customer {CustomerId}",
                customer.Id);
        }

        return new CreateCustomerWithAccountResponse(customer.Id, account.Id, account.AccountNumber);
    }
}