using BankRUs.Application.Interfaces;
using BankRUs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public class BankDbContext : DbContext, IUnitOfWork
{
    public BankDbContext(DbContextOptions<BankDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await base.SaveChangesAsync(cancellationToken);
    }
}