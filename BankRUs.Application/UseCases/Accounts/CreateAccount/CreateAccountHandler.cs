using BankRUs.Application.Interfaces;
using BankRUs.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BankRUs.Application.UseCases.Accounts.CreateAccount
{
    public class CreateAccountHandler
    {
        private readonly ICustomerRepository _customers;
        private readonly IAccountRepository _accounts;
        private readonly IAccountNumberGenerator _accountNumberGenerator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CreateAccountHandler> _logger;

        public CreateAccountHandler(
            ICustomerRepository customers,
            IAccountRepository accounts,
            IAccountNumberGenerator accountNumberGenerator,
            IUnitOfWork unitOfWork,
            IEmailSender emailSender,
            ILogger<CreateAccountHandler> logger)
        {
            _customers = customers;
            _accounts = accounts;
            _accountNumberGenerator = accountNumberGenerator;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<CreateAccountResult> HandleAsync(
            CreateAccountCommand command,
            CancellationToken cancellationToken)
        {
            // 1) Мини-валидация use-case уровня
            if (command.CustomerId == Guid.Empty)
                return CreateAccountResult.Fail("VALIDATION_ERROR", "CustomerId is required.");

            if (command.InitialDeposit < 0m)
                return CreateAccountResult.Fail("VALIDATION_ERROR", "InitialDeposit cannot be negative.");

            // 2) Проверяем, что клиент существует
            var customer = await _customers.GetByIdAsync(command.CustomerId, cancellationToken);
            if (customer is null)
                return CreateAccountResult.Fail("NOT_FOUND", "Customer not found.");

            // 3) Генерируем уникальный номер счёта
            var accountNumber = await _accountNumberGenerator.GenerateAsync(cancellationToken);

            // 4) Создаём доменную сущность
            BankAccount account;
            try
            {
                account = new BankAccount(customer.Id, accountNumber, command.InitialDeposit);
            }
            catch (ArgumentException ex)
            {
                // если домен выкинул ошибку (валидация)
                return CreateAccountResult.Fail("DOMAIN_VALIDATION_ERROR", ex.Message);
            }

            // 5) Persist + commit
            await _accounts.AddAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                var subject = "Välkommen till BankRUs!";
                var body = $@"
        <h2>Hej {customer.Name}!</h2>
        <p>Ditt bankkonto är skapat.</p>
        <p><b>Kontonummer:</b> {account.AccountNumber}</p>
        <p>Saldo: {account.Balance} SEK</p>";

                await _emailSender.SendAsync(customer.Email, subject, body, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send welcome email for customer {CustomerId}",
                    customer.Id);
                throw; // временно, чтобы увидеть ошибку в ответе/консоли

                // НЕ return Fail
            }

            // 6) Result
            return CreateAccountResult.Ok(
                accountId: account.Id,
                ownerId: account.OwnerId,
                accountNumber: account.AccountNumber,
                balance: account.Balance
            );
        }
    }
}