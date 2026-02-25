using BankRUs.Application.Interfaces;
using BankRUs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BankRUs.Infrastructure.Persistence;

namespace BankRUs.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly BankDbContext _context;

        public AccountRepository(BankDbContext context)
        {
            _context = context;
        }

        public async Task<BankAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.BankAccounts
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<BankAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken)
        {
            return await _context.BankAccounts
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);
        }

        public async Task<bool> AccountNumberExistsAsync(string accountNumber, CancellationToken cancellationToken)
        {
            return await _context.BankAccounts
                .AnyAsync(a => a.AccountNumber == accountNumber, cancellationToken);
        }

        public async Task AddAsync(BankAccount account, CancellationToken cancellationToken)
        {
            await _context.BankAccounts.AddAsync(account, cancellationToken);
        }
    }
}