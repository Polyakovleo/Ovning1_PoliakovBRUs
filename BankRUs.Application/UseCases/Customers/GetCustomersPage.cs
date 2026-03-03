using BankRUs.Application.Common.Paging;
using BankRUs.Application.Interfaces;

namespace BankRUs.Application.UseCases.Customers;

public record GetCustomersPageQuery(int Page, int PageSize, string? Ssn);

public class GetCustomersPage
{
    private readonly ICustomerRepository _customers;

    public GetCustomersPage(ICustomerRepository customers)
    {
        _customers = customers;
    }

    public async Task<PagedResult<CustomerListItemDto>> ExecuteAsync(
        GetCustomersPageQuery query,
        CancellationToken ct)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 1 : query.PageSize;
        var ssn = string.IsNullOrWhiteSpace(query.Ssn) ? null : query.Ssn.Trim();

        var (items, totalCount) = await _customers.GetPageAsync(page, pageSize, ssn, ct);

        var list = items.Select(c => new CustomerListItemDto(
            c.Id,
            c.Name,
            c.Email,
            c.PersonalNumber,
            c.Accounts?.Count ?? 0
        )).ToList();

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<CustomerListItemDto>
        {
            Data = list,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = totalPages
        };
    }
}

