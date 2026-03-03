using BankRUs.Application.Interfaces;
using BankRUs.Domain.Entities;
using BankRUs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankRUs.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly BankDbContext _context;

        public CustomerRepository(BankDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Customer?> GetByIdWithAccountsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Customers
                .AsNoTracking()
                .Include(c => c.Accounts)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Customer?> GetByPersonalNumberAsync(string personalNumber, CancellationToken cancellationToken)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.PersonalNumber == personalNumber, cancellationToken);
        }

        public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
        }

        public async Task AddAsync(Customer customer, CancellationToken cancellationToken)
        {
            await _context.Customers.AddAsync(customer, cancellationToken);
        }

        public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct)
        {
            return await _context.Customers
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Include(c => c.Accounts)
                .ToListAsync(ct);
        }

        public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPageAsync(
            int page,
            int pageSize,
            string? ssnFilter,
            CancellationToken ct)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;

            IQueryable<Customer> query = _context.Customers;

            if (!string.IsNullOrWhiteSpace(ssnFilter))
            {
                var normalized = ssnFilter.Trim();
                query = query.Where(c => c.PersonalNumber.StartsWith(normalized));
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Include(c => c.Accounts)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public Task DeleteAsync(Customer customer, CancellationToken ct)
        {
            _context.Customers.Remove(customer);
            return Task.CompletedTask;
        }
    }
}