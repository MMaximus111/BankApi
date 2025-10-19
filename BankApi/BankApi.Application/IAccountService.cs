using BankApi.Application.TransferObjects;

namespace BankApi.Application;

public interface IAccountService
{
    Task CreateTransactionAsync(CreateTransactionDto dto, CancellationToken cancellationToken);
    
    Task<AccountDto> CreateAccountAsync(CreateAccountDto dto, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AccountDto>> GetAllAccountsAsync(CancellationToken cancellationToken);
    
    Task<AccountDto> GetAccountByPhoneAsync(string? phone, CancellationToken cancellationToken);
}