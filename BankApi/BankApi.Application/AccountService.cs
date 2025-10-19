using BankApi.Application.Common;
using BankApi.Application.TransferObjects;
using BankApi.Core.Dictionaries;
using BankApi.Core.Entities;
using BankApi.Core.Exceptions;

namespace BankApi.Application;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;

    public AccountService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task CreateTransactionAsync(CreateTransactionDto dto, CancellationToken cancellationToken)
    {
        Account? account = await _accountRepository.GetAccountByIdAsync(dto.ToAccountId, cancellationToken);

        if (account is null)
        {
            throw new BankBusinessException($"Account with id {dto.ToAccountId} not found.");
        }

        TransactionType type = dto.FromAccountId.HasValue ? TransactionType.AccountAccountTransfer : TransactionType.AtmDeposit;

        if (type == TransactionType.AccountAccountTransfer)
        {
            Account? sourceAccount = await _accountRepository.GetAccountByIdAsync(dto.FromAccountId!.Value, cancellationToken);
            
            if (sourceAccount is null)
            {
                throw new BankBusinessException($"Source account with id {dto.FromAccountId.Value} not found.");
            }

            if (sourceAccount.GetBalance() < dto.Amount)
            {
                throw new BankBusinessException("Insufficient funds in the source account.");
            }
            
            sourceAccount.AddTransaction(new Transaction(-dto.Amount, DateTime.UtcNow, dto.FromAccountId.Value, type) );
        }
        
        account.AddTransaction(new Transaction(dto.Amount, DateTime.UtcNow, dto.ToAccountId, type));

        await _accountRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            throw new BankBusinessException("Phone number must be provided.");
        }
        
        Account? existedAccount = await _accountRepository.GetAccountByPhoneAsync(dto.PhoneNumber, cancellationToken);

        if (existedAccount is not null)
        {
            throw new BankBusinessException($"Account with phone number {dto.PhoneNumber} already exists.");
        }
        
        Account account = new Account(dto.PhoneNumber!);

        await _accountRepository.CreateAccountAsync(account, cancellationToken);
        
        return new AccountDto
        {
            Id = account.Id,
            PhoneNumber = account.PhoneNumber,
            Balance = account.GetBalance()
        };
    }

    public async Task<IReadOnlyCollection<AccountDto>> GetAllAccountsAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Account> accounts = await _accountRepository.GetAllAccountsAsync(cancellationToken);
        
        return accounts.Select(a => new AccountDto
        {
            Id = a.Id,
            PhoneNumber = a.PhoneNumber,
            Balance = a.GetBalance()
        }).ToList();
    }

    public async Task<AccountDto> GetAccountByPhoneAsync(string? phone, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new BankBusinessException("Phone number must be provided.");
        }
        
        Account? account = await _accountRepository.GetAccountByPhoneAsync(phone, cancellationToken);

        if (account is null)
        {
            throw new BankBusinessException($"Account with phone number {phone} not found.");
        }
        
        return new AccountDto
        {
            Id = account.Id,
            PhoneNumber = account.PhoneNumber,
            Balance = account.GetBalance()
        };
    }
}