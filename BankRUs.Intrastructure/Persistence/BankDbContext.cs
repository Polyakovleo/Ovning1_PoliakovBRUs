using BankRUs.Application.Interfaces;
using BankRUs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankRUs.Infrastructure.Persistence;

public class BankDbContext : DbContext, IUnitOfWork
{
    public BankDbContext(DbContextOptions<BankDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Email).IsRequired().HasMaxLength(320);
            e.Property(x => x.PersonalNumber).IsRequired().HasMaxLength(32);

            // Уникальность (чтобы не было дублей)
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.PersonalNumber).IsUnique();

            // Связь 1 Customer -> many Accounts
            e.HasMany(x => x.Accounts)
             .WithOne(x => x.Owner)
             .HasForeignKey(x => x.OwnerId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);
        });

        // BankAccount
        modelBuilder.Entity<BankAccount>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.AccountNumber).IsRequired().HasMaxLength(34);
            e.HasIndex(x => x.AccountNumber).IsUnique();

            // Деньги: лучше фиксировать точность
            e.Property(x => x.Balance).HasPrecision(18, 2);
        });
    }

    public new Task SaveChangesAsync(CancellationToken cancellationToken)
        => base.SaveChangesAsync(cancellationToken);
}