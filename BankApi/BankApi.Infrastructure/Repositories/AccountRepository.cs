using BankApi.Application.Common;
using BankApi.Core.Entities;
using BankApi.Infrastructure.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly BankDbContext _context;

    public AccountRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateAccountAsync(Account account, CancellationToken cancellationToken)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
        
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Account>> GetAllAccountsAsync(CancellationToken cancellationToken)
    {
        return await _context.Accounts.Include(x => x.Transactions).ToListAsync(cancellationToken);
    }

    public async Task<Account?> GetAccountByPhoneAsync(string phone, CancellationToken cancellationToken)
    {
        return await _context.Accounts.Include(x => x.Transactions).FirstOrDefaultAsync(a => a.PhoneNumber == phone, cancellationToken);
    }

    public async Task<Account?> GetAccountByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Accounts.Include(x => x.Transactions).FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}