namespace BankApi.Core.Exceptions;

public class BankBusinessException : Exception
{
    public BankBusinessException(string message) : base(message)
    {
    }
}