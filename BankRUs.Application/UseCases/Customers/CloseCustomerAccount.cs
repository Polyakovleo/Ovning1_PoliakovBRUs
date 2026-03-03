using BankRUs.Application.Common.Exceptions;
using BankRUs.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace BankRUs.Application.UseCases.Customers;

public record CloseCustomerAccountCommand(Guid CustomerId);

public class CloseCustomerAccount
{
    private readonly ICustomerRepository _customers;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CloseCustomerAccount> _logger;

    public CloseCustomerAccount(ICustomerRepository customers, IUnitOfWork uow, ILogger<CloseCustomerAccount> logger)
    {
        _customers = customers;
        _uow = uow;
        _logger = logger;
    }

    public async Task HandleAsync(CloseCustomerAccountCommand command, CancellationToken ct)
    {
        if (command.CustomerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required.", nameof(command.CustomerId));

        var customer = await _customers.GetByIdAsync(command.CustomerId, ct);
        if (customer is null)
            throw new NotFoundException("Customer not found.");

        await _customers.DeleteAsync(customer, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Closed customer account {CustomerId} and deleted related bank accounts",
            customer.Id);
    }
}

