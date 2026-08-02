namespace Dsw2026Tpi.CrossCutting.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string errorCode, string message)
        : base(message, errorCode)
    {
    }
}