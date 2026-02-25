using BankRUs.Domain.Entities;

namespace BankRUs.Application.Interfaces
{
    public interface IAccountRepository
    {
        Task<BankAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<BankAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken);

        Task<bool> AccountNumberExistsAsync(string accountNumber, CancellationToken cancellationToken);

        Task AddAsync(BankAccount account, CancellationToken cancellationToken);

        //Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
