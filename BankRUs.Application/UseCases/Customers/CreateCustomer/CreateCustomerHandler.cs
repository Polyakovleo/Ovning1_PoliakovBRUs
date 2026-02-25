using BankRUs.Application.Interfaces;
using BankRUs.Domain.Entities;

namespace BankRUs.Application.UseCases.Customers.CreateCustomer
{
    public class CreateCustomerHandler
    {
        private readonly ICustomerRepository _customers;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCustomerHandler(ICustomerRepository customers, IUnitOfWork unitOfWork)
        {
            _customers = customers;
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateCustomerResult> HandleAsync(
            CreateCustomerCommand command,
            CancellationToken cancellationToken)
        {
            // 1) Мини-валидация на уровне use-case (домен тоже валидирует в конструкторе)
            if (string.IsNullOrWhiteSpace(command.Name) ||
                string.IsNullOrWhiteSpace(command.Email) ||
                string.IsNullOrWhiteSpace(command.PersonalNumber))
            {
                return CreateCustomerResult.Fail("VALIDATION_ERROR", "Name, Email and PersonalNumber are required.");
            }
            // 2) Нормализация (ВОТ СЮДА)
            var name = command.Name.Trim();
            var email = command.Email.Trim();
            var personalNumber = command.PersonalNumber.Trim();

            // 3) Уникальность (по правилам)
            var existingByPn = await _customers.GetByPersonalNumberAsync(personalNumber, cancellationToken);
            if (existingByPn is not null)
                return CreateCustomerResult.Fail("PERSONAL_NUMBER_EXISTS", "A customer with this personal number already exists.");

            var existingByEmail = await _customers.GetByEmailAsync(email, cancellationToken);
            if (existingByEmail is not null)
                return CreateCustomerResult.Fail("EMAIL_EXISTS", "A customer with this email already exists.");

            // 3) Создание доменной сущности (валидация внутри Customer constructor)
            Customer customer;
            try
            {
                customer = new Customer(name, email, personalNumber);
            }
            catch (ArgumentException ex)
            {
                return CreateCustomerResult.Fail("DOMAIN_VALIDATION_ERROR", ex.Message);
            }

            // 4) Persist
            await _customers.AddAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5) Result
            return CreateCustomerResult.Ok(customer.Id);
        }
    }
}
