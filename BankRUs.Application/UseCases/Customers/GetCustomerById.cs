using BankRUs.Application.Common.Exceptions;
using BankRUs.Application.Interfaces;

namespace BankRUs.Application.UseCases.Customers;

public class GetCustomerById
{
    private readonly ICustomerRepository _customers;

    public GetCustomerById(ICustomerRepository customers)
    {
        _customers = customers;
    }

    public async Task<CustomerDetailDto> ExecuteAsync(Guid id, CancellationToken ct)
    {
        var customer = await _customers.GetByIdWithAccountsAsync(id, ct);

        if (customer is null)
            throw new NotFoundException("Customer not found.");

        var accounts = (customer.Accounts ?? Array.Empty<Domain.Entities.BankAccount>())
            .Select(a => new BankAccountDto(
                a.Id,
                a.AccountNumber,
                a.Balance))
            .ToList();

        return new CustomerDetailDto(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.PersonalNumber,
            accounts);
    }
}

