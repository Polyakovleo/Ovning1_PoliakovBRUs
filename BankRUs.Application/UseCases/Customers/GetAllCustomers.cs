using BankRUs.Application.Interfaces;

namespace BankRUs.Application.UseCases.Customers;

public record CustomerListItemDto(
    Guid Id,
    string Name,
    string Email,
    string PersonalNumber,
    int AccountsCount
);

public class GetAllCustomers
{
    private readonly ICustomerRepository _customers;

    public GetAllCustomers(ICustomerRepository customers) => _customers = customers;

    public async Task<IReadOnlyList<CustomerListItemDto>> ExecuteAsync(CancellationToken ct)
    {
        var list = await _customers.GetAllAsync(ct);

        return list.Select(c => new CustomerListItemDto(
            c.Id,
            c.Name,
            c.Email,
            c.PersonalNumber,
            c.Accounts?.Count ?? 0
        )).ToList();
    }
}