using BankRUs.Application.Common.Exceptions;
using BankRUs.Application.Interfaces;

namespace BankRUs.Application.UseCases.Customers;

public record UpdateCustomerDetailsCommand(
    Guid CustomerId,
    string? Name,
    string? Email,
    string? PersonalNumber
);

public class UpdateCustomerDetails
{
    private readonly ICustomerRepository _customers;
    private readonly IUnitOfWork _uow;

    public UpdateCustomerDetails(ICustomerRepository customers, IUnitOfWork uow)
    {
        _customers = customers;
        _uow = uow;
    }

    public async Task HandleAsync(UpdateCustomerDetailsCommand command, CancellationToken ct)
    {
        if (command.CustomerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required.", nameof(command.CustomerId));

        var customer = await _customers.GetByIdAsync(command.CustomerId, ct);
        if (customer is null)
            throw new NotFoundException("Customer not found.");

        var name = command.Name?.Trim();
        var email = command.Email?.Trim();
        var personalNumber = command.PersonalNumber?.Trim();

        if (name is null && email is null && personalNumber is null)
            throw new DomainValidationException("At least one field must be provided.");

        if (email is not null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new DomainValidationException("Email cannot be empty.");

            var existingByEmail = await _customers.GetByEmailAsync(email, ct);
            if (existingByEmail is not null && existingByEmail.Id != customer.Id)
                throw new DomainValidationException("A customer with this email already exists.");
        }

        if (personalNumber is not null)
        {
            if (string.IsNullOrWhiteSpace(personalNumber))
                throw new DomainValidationException("Personal number cannot be empty.");

            var existingByPn = await _customers.GetByPersonalNumberAsync(personalNumber, ct);
            if (existingByPn is not null && existingByPn.Id != customer.Id)
                throw new DomainValidationException("A customer with this personal number already exists.");
        }

        try
        {
            if (name is not null)
                customer.ChangeName(name);

            if (email is not null)
                customer.ChangeEmail(email);

            if (personalNumber is not null)
                customer.ChangePersonalNumber(personalNumber);
        }
        catch (ArgumentException ex)
        {
            throw new DomainValidationException(ex.Message);
        }

        await _uow.SaveChangesAsync(ct);
    }
}

