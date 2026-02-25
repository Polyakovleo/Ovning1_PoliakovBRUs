using BankRUs.Application.Interfaces;
using BankRUs.Domain.Entities;

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

    public CreateCustomerWithAccount(
        ICustomerRepository customers,
        IAccountRepository accounts,
        IAccountNumberGenerator accountNumberGenerator,
        IUnitOfWork uow)
    {
        _customers = customers;
        _accounts = accounts;
        _accountNumberGenerator = accountNumberGenerator;
        _uow = uow;
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
            throw new InvalidOperationException("Customer with this personal number already exists.");

        var existingByEmail = await _customers.GetByEmailAsync(req.Email, ct);
        if (existingByEmail is not null)
            throw new InvalidOperationException("Customer with this email already exists.");

        // Создаем Customer
        var customer = new Customer(req.Name, req.Email, req.PersonalNumber);
        await _customers.AddAsync(customer, ct);

        // Генерим номер аккаунта + создаём BankAccount, привязанный к customer.Id
        var accountNumber = await _accountNumberGenerator.GenerateAsync(ct);

        // (опционально) если генератор не гарантирует уникальность:
        if (await _accounts.AccountNumberExistsAsync(accountNumber, ct))
            throw new InvalidOperationException("Generated account number already exists. Try again.");

        var account = new BankAccount(customer.Id, accountNumber, req.InitialBalance);
        await _accounts.AddAsync(account, ct);

        // Один коммит на всё
        await _uow.SaveChangesAsync(ct);

        return new CreateCustomerWithAccountResponse(customer.Id, account.Id, account.AccountNumber);
    }
}