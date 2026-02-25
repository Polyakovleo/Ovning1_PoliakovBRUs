

namespace BankRUs.Domain.Entities
{
    public class BankAccount
    {
        // EF Core needs a parameterless constructor
        private BankAccount() { }
        public BankAccount(Guid ownerId, string accountNumber, decimal initialBalance = 0m)
        {
            if (ownerId == Guid.Empty)
                throw new ArgumentException("OwnerId is required.", nameof(ownerId));

            accountNumber = NormalizeRequired(accountNumber, nameof(accountNumber));
            if (initialBalance < 0m)
                throw new ArgumentOutOfRangeException(nameof(initialBalance), "Initial balance cannot be negative.");

            Id = Guid.NewGuid();
            OwnerId = ownerId;
            AccountNumber = accountNumber;
            Balance = initialBalance;
        }
        public Guid Id { get; private set; }

        public Guid OwnerId { get; private set; }
        public Customer? Owner { get; private set; } // Navigation (EF)

        public string AccountNumber { get; private set; } = default!;
        public decimal Balance { get; private set; }
        public void Deposit(decimal amount)
        {
            if (amount <= 0m)
                throw new ArgumentOutOfRangeException(nameof(amount), "Deposit amount must be greater than 0.");

            Balance += amount;
        }
        public void Withdraw(decimal amount)
        {
            if (amount <= 0m)
                throw new ArgumentOutOfRangeException(nameof(amount), "Withdraw amount must be greater than 0.");

            if (amount > Balance)
                throw new InvalidOperationException("Insufficient funds.");

            Balance -= amount;
        }
        private static string NormalizeRequired(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value is required.", paramName);

            return value.Trim();
        }
    }
}
