using BankApi.Application;
using BankApi.Application.TransferObjects;
using BankApi.Core.Dictionaries;
using BankApi.Core.Entities;
using BankApi.Core.Exceptions;
using BankApi.Infrastructure.EntityFrameworkCore;
using BankApi.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Tests;

public class AccountServiceTests
{
    [Fact]
    public async Task CreateAccountAsync_ShouldCreateAccount_WhenPhoneIsValid()
    {
        // Arrange
        BankDbContext dbContext = CreateInMemoryDbContext();
        AccountRepository repository = new AccountRepository(dbContext);
        AccountService service = new AccountService(repository);

        CreateAccountDto dto = new CreateAccountDto
        {
            PhoneNumber = "1234567890"
        };

        // Act
        AccountDto result = await service.CreateAccountAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.PhoneNumber.Should().Be(dto.PhoneNumber);
        result.Balance.Should().Be(0m);

        Account? accountInDb = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == result.Id);
        accountInDb.Should().NotBeNull();
        accountInDb.PhoneNumber.Should().Be(dto.PhoneNumber);
    }

    [Fact]
    public async Task CreateAccountAsync_ShouldThrow_WhenPhoneIsEmpty()
    {
        // Arrange
        BankDbContext dbContext = CreateInMemoryDbContext();
        AccountRepository repository = new AccountRepository(dbContext);
        AccountService service = new AccountService(repository);

        CreateAccountDto dto = new CreateAccountDto
        {
            PhoneNumber = ""
        };

        // Act
        Func<Task> act = async () => await service.CreateAccountAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BankBusinessException>()
            .WithMessage("Phone number must be provided.");
    }

    [Fact]
    public async Task CreateAccountAsync_ShouldThrow_WhenPhoneAlreadyExists()
    {
        // Arrange
        BankDbContext dbContext = CreateInMemoryDbContext();
        AccountRepository repository = new AccountRepository(dbContext);
        AccountService service = new AccountService(repository);

        Account existingAccount = new Account("1234567890");
        await repository.CreateAccountAsync(existingAccount, CancellationToken.None);

        CreateAccountDto dto = new CreateAccountDto
        {
            PhoneNumber = "1234567890"
        };

        // Act
        Func<Task> act = async () => await service.CreateAccountAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BankBusinessException>()
            .WithMessage("Account with phone number 1234567890 already exists.");
    }
    
     [Fact]
        public async Task CreateTransactionAsync_ShouldDeposit_WhenFromAccountIdIsNull()
        {
            // Arrange
            BankDbContext dbContext = CreateInMemoryDbContext();
            AccountRepository repository = new AccountRepository(dbContext);
            AccountService service = new AccountService(repository);

            Account toAccount = new Account("1234567890");
            await repository.CreateAccountAsync(toAccount, CancellationToken.None);

            CreateTransactionDto dto = new CreateTransactionDto
            {
                ToAccountId = toAccount.Id,
                Amount = 100m,
                FromAccountId = null
            };

            // Act
            await service.CreateTransactionAsync(dto, CancellationToken.None);

            // Assert
            Account accountInDb = await dbContext.Accounts.Include(a => a.Transactions).FirstAsync(a => a.Id == toAccount.Id);
            accountInDb.Transactions.Should().HaveCount(1);
            accountInDb.GetBalance().Should().Be(100m);

            Transaction transaction = accountInDb.Transactions.First();
            transaction.Amount.Should().Be(100m);
            transaction.TransactionType.Should().Be(TransactionType.AtmDeposit);
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldTransfer_WhenFromAccountIdIsSet()
        {
            // Arrange
            BankDbContext dbContext = CreateInMemoryDbContext();
            AccountRepository repository = new AccountRepository(dbContext);
            AccountService service = new AccountService(repository);

            Account sourceAccount = new Account("1111111111");
            Account targetAccount = new Account("2222222222");

            await repository.CreateAccountAsync(sourceAccount, CancellationToken.None);
            await repository.CreateAccountAsync(targetAccount, CancellationToken.None);

            sourceAccount.AddTransaction(new Transaction(200m, DateTime.UtcNow, sourceAccount.Id, TransactionType.AtmDeposit));
            await repository.SaveChangesAsync(CancellationToken.None);

            CreateTransactionDto dto = new CreateTransactionDto
            {
                FromAccountId = sourceAccount.Id,
                ToAccountId = targetAccount.Id,
                Amount = 150m
            };

            // Act
            await service.CreateTransactionAsync(dto, CancellationToken.None);

            // Assert
            Account sourceInDb = await dbContext.Accounts.Include(a => a.Transactions).FirstAsync(a => a.Id == sourceAccount.Id);
            Account targetInDb = await dbContext.Accounts.Include(a => a.Transactions).FirstAsync(a => a.Id == targetAccount.Id);

            sourceInDb.Transactions.Should().HaveCount(2);
            sourceInDb.GetBalance().Should().Be(50m);

            targetInDb.Transactions.Should().HaveCount(1);
            targetInDb.GetBalance().Should().Be(150m);

            Transaction sourceTransaction = sourceInDb.Transactions.Last();
            sourceTransaction.Amount.Should().Be(-150m);
            sourceTransaction.TransactionType.Should().Be(TransactionType.AccountAccountTransfer);

            Transaction targetTransaction = targetInDb.Transactions.First();
            targetTransaction.Amount.Should().Be(150m);
            targetTransaction.TransactionType.Should().Be(TransactionType.AccountAccountTransfer);
        }

        [Fact]
        public async Task GetAllAccountsAsync_ShouldReturnAllAccounts()
        {
            // Arrange
            BankDbContext dbContext = CreateInMemoryDbContext();
            AccountRepository repository = new AccountRepository(dbContext);
            AccountService service = new AccountService(repository);

            Account account1 = new Account("1111111111");
            Account account2 = new Account("2222222222");

            await repository.CreateAccountAsync(account1, CancellationToken.None);
            await repository.CreateAccountAsync(account2, CancellationToken.None);

            // Act
            IReadOnlyCollection<AccountDto> result = await service.GetAllAccountsAsync(CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.Select(a => a.PhoneNumber).Should().Contain(new[] { "1111111111", "2222222222" });
        }

        [Fact]
        public async Task GetAccountByPhoneAsync_ShouldReturnAccount_WhenPhoneExists()
        {
            // Arrange
            BankDbContext dbContext = CreateInMemoryDbContext();
            AccountRepository repository = new AccountRepository(dbContext);
            AccountService service = new AccountService(repository);

            Account account = new Account("1234567890");
            await repository.CreateAccountAsync(account, CancellationToken.None);

            // Act
            AccountDto result = await service.GetAccountByPhoneAsync("1234567890", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.PhoneNumber.Should().Be("1234567890");
        }

        [Fact]
        public async Task GetAccountByPhoneAsync_ShouldThrow_WhenPhoneDoesNotExist()
        {
            // Arrange
            BankDbContext dbContext = CreateInMemoryDbContext();
            AccountRepository repository = new AccountRepository(dbContext);
            AccountService service = new AccountService(repository);

            // Act
            Func<Task> act = async () => await service.GetAccountByPhoneAsync("0000000000", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BankBusinessException>()
                .WithMessage("Account with phone number 0000000000 not found.");
        }
        
        [Fact]
        public async Task CreateTransactionAsync_ShouldThrow_WhenToAccountDoesNotExist()
        {
            // Arrange
            BankDbContext dbContext = CreateInMemoryDbContext();
            AccountRepository repository = new AccountRepository(dbContext);
            AccountService service = new AccountService(repository);

            CreateTransactionDto dto = new CreateTransactionDto
            {
                ToAccountId = 999,
                Amount = 100m
            };

            // Act
            Func<Task> act = async () => await service.CreateTransactionAsync(dto, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BankBusinessException>()
                .WithMessage("Account with id 999 not found.");
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldThrow_WhenSourceAccountDoesNotExist()
        {
            // Arrange
            BankDbContext dbContext = CreateInMemoryDbContext();
            AccountRepository repository = new AccountRepository(dbContext);
            AccountService service = new AccountService(repository);

            Account toAccount = new Account("1234567890");
            await repository.CreateAccountAsync(toAccount, CancellationToken.None);

            CreateTransactionDto dto = new CreateTransactionDto
            {
                FromAccountId = 999,
                ToAccountId = toAccount.Id,
                Amount = 50m
            };

            // Act
            Func<Task> act = async () => await service.CreateTransactionAsync(dto, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BankBusinessException>()
                .WithMessage("Source account with id 999 not found.");
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldThrow_WhenInsufficientFunds()
        {
            // Arrange
            BankDbContext dbContext = CreateInMemoryDbContext();
            AccountRepository repository = new AccountRepository(dbContext);
            AccountService service = new AccountService(repository);

            Account sourceAccount = new Account("1111111111");
            Account toAccount = new Account("2222222222");

            await repository.CreateAccountAsync(sourceAccount, CancellationToken.None);
            await repository.CreateAccountAsync(toAccount, CancellationToken.None);

            sourceAccount.AddTransaction(new Transaction(100m, DateTime.UtcNow, sourceAccount.Id, TransactionType.AtmDeposit));
            await repository.SaveChangesAsync(CancellationToken.None);

            CreateTransactionDto dto = new CreateTransactionDto
            {
                FromAccountId = sourceAccount.Id,
                ToAccountId = toAccount.Id,
                Amount = 200m
            };

            // Act
            Func<Task> act = async () => await service.CreateTransactionAsync(dto, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BankBusinessException>()
                .WithMessage("Insufficient funds in the source account.");
        }
        
        [Fact]
        public async Task GetAccountByPhoneAsync_ShouldThrow_WhenPhoneIsNullOrEmpty()
        {
            // Arrange
            BankDbContext dbContext = CreateInMemoryDbContext();
            AccountRepository repository = new AccountRepository(dbContext);
            AccountService service = new AccountService(repository);

            string phone = "";

            // Act
            Func<Task> act = async () => await service.GetAccountByPhoneAsync(phone, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BankBusinessException>()
                .WithMessage("Phone number must be provided.");
        }
        
        [Fact]
        public async Task GetAllAccountsAsync_ShouldReturnEmptyCollection_WhenNoAccountsExist()
        {
            // Arrange
            BankDbContext dbContext = CreateInMemoryDbContext();
            AccountRepository repository = new AccountRepository(dbContext);
            AccountService service = new AccountService(repository);

            // Act
            IReadOnlyCollection<AccountDto> result = await service.GetAllAccountsAsync(CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }
    
    private static BankDbContext CreateInMemoryDbContext()
    {
        DbContextOptions<BankDbContext> options = new DbContextOptionsBuilder<BankDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BankDbContext(options);
    }
}