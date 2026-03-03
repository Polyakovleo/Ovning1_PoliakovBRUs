
using BankRUs.Domain.Entities;

namespace BankRUs.Application.Interfaces
{

        public interface ICustomerRepository
        {
            Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

            Task<Customer?> GetByIdWithAccountsAsync(Guid id, CancellationToken cancellationToken);

            Task<Customer?> GetByPersonalNumberAsync(string personalNumber, CancellationToken cancellationToken);

            Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken);

            Task AddAsync(Customer customer, CancellationToken cancellationToken);

            Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken);

            Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPageAsync(
                int page,
                int pageSize,
                string? ssnFilter,
                CancellationToken cancellationToken);

            Task DeleteAsync(Customer customer, CancellationToken cancellationToken);

            //Task SaveChangesAsync(CancellationToken cancellationToken);
    }
    
}
