using BankApi.Core.Dictionaries;
using BankApi.Core.Entities.Base;

namespace BankApi.Core.Entities;

public class Transaction : IDomainEntity
{
    public Transaction(decimal amount, DateTime transactionDate, int accountId, TransactionType transactionType)
    {
        Amount = amount;
        TransactionDate = transactionDate;
        AccountId = accountId;
        TransactionType = transactionType;
    }

    public long Id { get; private set; }
    
    public decimal Amount { get; private set; }
    
    public DateTime TransactionDate { get; private set; }
    
    public int AccountId { get; private set; }
    
    public TransactionType TransactionType { get; private set; }
}