namespace BankRUs.Application.Interfaces
{
    public interface IAccountNumberGenerator
    {
        Task<string> GenerateAsync(CancellationToken cancellationToken);
    }
}
