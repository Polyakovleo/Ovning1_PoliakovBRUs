namespace BankRUs.Application.UseCases.Accounts.CreateAccount
{
    public class CreateAccountResult
    {
        public bool Success { get; private set; }
        public string? ErrorCode { get; private set; }
        public string? ErrorMessage { get; private set; }

        public Guid? AccountId { get; private set; }
        public Guid? OwnerId { get; private set; }
        public string? AccountNumber { get; private set; }
        public decimal? Balance { get; private set; }

        private CreateAccountResult() { }

        public static CreateAccountResult Ok(Guid accountId, Guid ownerId, string accountNumber, decimal balance) =>
            new CreateAccountResult
            {
                Success = true,
                AccountId = accountId,
                OwnerId = ownerId,
                AccountNumber = accountNumber,
                Balance = balance
            };

        public static CreateAccountResult Fail(string errorCode, string errorMessage) =>
            new CreateAccountResult
            {
                Success = false,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
    }
}