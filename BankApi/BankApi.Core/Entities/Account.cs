using BankApi.Core.Entities.Base;

namespace BankApi.Core.Entities;

public class Account : IAggregateRoot
{
    public Account(string phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }

    public int Id { get; private set; }
    
    public string PhoneNumber { get; private set; }
    
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
    
    public decimal GetBalance() => Transactions.Sum(t => t.Amount);

    public void AddTransaction(Transaction transaction)
    {
        Transactions.Add(transaction);
    }
}