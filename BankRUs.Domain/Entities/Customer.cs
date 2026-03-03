
namespace BankRUs.Domain.Entities
{
    public class Customer
    {
        // EF Core needs a parameterless constructor
        private Customer() { }
        public Customer(string name, string email, string personalNumber)
        {
            Id = Guid.NewGuid();

            Name = NormalizeRequired(name, nameof(name));
            Email = ValidateEmail(email);
            PersonalNumber = ValidatePersonalNumber(personalNumber);

            Accounts = new List<BankAccount>();
        }

        public Guid Id { get; private set; }

        public string Name { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public string PersonalNumber { get; private set; } = default!;

        // Navigation (EF)
        public ICollection<BankAccount> Accounts { get; private set; } = new List<BankAccount>();

        public void ChangeEmail(string email) => Email = ValidateEmail(email);

        public void ChangeName(string name) => Name = NormalizeRequired(name, nameof(name));

        public void ChangePersonalNumber(string personalNumber) =>
            PersonalNumber = ValidatePersonalNumber(personalNumber);

        private static string NormalizeRequired(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value is required.", paramName);

            return value.Trim();
        }
        private static string ValidateEmail(string email)
        {
            email = NormalizeRequired(email, nameof(email));

            // Simple, enough for homework. Real projects: dedicated validation.
            if (!email.Contains("@") || email.StartsWith("@") || email.EndsWith("@"))
                throw new ArgumentException("Invalid email format.", nameof(email));

            return email;
        }
        private static string ValidatePersonalNumber(string personalNumber)
        {
            personalNumber = NormalizeRequired(personalNumber, nameof(personalNumber));

            // Keep it flexible: allow digits and separators like '-'.
            // Example formats: "YYYYMMDD-XXXX" or "YYMMDD-XXXX"
            var digitsCount = 0;
            foreach (var ch in personalNumber)
            {
                if (char.IsDigit(ch)) digitsCount++;
                else if (ch is '-' or '+') continue;
                else throw new ArgumentException("Personal number contains invalid characters.", nameof(personalNumber));
            }

            if (digitsCount != 10 && digitsCount != 12)
                throw new ArgumentException("Personal number must contain 10 or 12 digits.", nameof(personalNumber));

            return personalNumber;
        }
    }
}
