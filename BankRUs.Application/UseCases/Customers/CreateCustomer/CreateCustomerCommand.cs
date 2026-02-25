namespace BankRUs.Application.UseCases.Customers.CreateCustomer
{
    public class CreateCustomerCommand
    {
        public string Name { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string PersonalNumber { get; init; } = default!;
    }
}