using BankApi.Core.Dictionaries;

namespace BankApi.Application.TransferObjects;

public record CreateTransactionDto
{
    public int? FromAccountId { get; init; }
    
    public int ToAccountId { get; init; }

    public decimal Amount { get; init; }
    
    public TransactionType Type { get; init; }
}