
namespace BankRUs.Application.UseCases.Accounts.CreateAccount
{
    public class CreateAccountCommand
    {
        public Guid CustomerId { get; init; }
        public decimal InitialDeposit { get; init; } // можно 0
    }
}
