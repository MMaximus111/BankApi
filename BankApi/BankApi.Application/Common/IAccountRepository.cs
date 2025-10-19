using BankApi.Core.Entities;

namespace BankApi.Application.Common;

public interface IAccountRepository
{
    public Task SaveChangesAsync(CancellationToken cancellationToken);

    public Task CreateAccountAsync(Account account, CancellationToken cancellationToken);

    public Task<IReadOnlyCollection<Account>> GetAllAccountsAsync(CancellationToken cancellationToken);

    public Task<Account?> GetAccountByPhoneAsync(string phone, CancellationToken cancellationToken);
    
    public Task<Account?> GetAccountByIdAsync(int id, CancellationToken cancellationToken);
}