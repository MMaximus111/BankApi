namespace BankApi.Application.TransferObjects;

public record CreateAccountDto
{
    public string? PhoneNumber { get; init; }
}