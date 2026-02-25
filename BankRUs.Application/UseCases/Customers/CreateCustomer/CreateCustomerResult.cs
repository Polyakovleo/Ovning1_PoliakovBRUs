namespace BankRUs.Application.UseCases.Customers.CreateCustomer
{
    public class CreateCustomerResult
    {
        public bool Success { get; private set; }
        public Guid? CustomerId { get; private set; }
        public string? ErrorCode { get; private set; }
        public string? ErrorMessage { get; private set; }

        private CreateCustomerResult() { }

        public static CreateCustomerResult Ok(Guid customerId) =>
            new CreateCustomerResult { Success = true, CustomerId = customerId };

        public static CreateCustomerResult Fail(string errorCode, string errorMessage) =>
            new CreateCustomerResult { Success = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
    }
}
