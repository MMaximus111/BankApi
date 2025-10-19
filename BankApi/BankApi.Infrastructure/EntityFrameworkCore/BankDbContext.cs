using BankApi.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Infrastructure.EntityFrameworkCore;

public class BankDbContext : DbContext
{
    public BankDbContext(DbContextOptions<BankDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Account> Accounts => Set<Account>();
}