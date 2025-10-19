namespace BankApi.Application.TransferObjects;

public record AccountDto
{
    public required int Id { get; init; }
    
    public required string PhoneNumber { get; init; }
    public required decimal Balance { get; init; }
}