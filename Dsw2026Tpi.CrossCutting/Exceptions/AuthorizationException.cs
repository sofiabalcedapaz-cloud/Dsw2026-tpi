using Dsw2026Tpi.CrossCutting.Resources;

namespace Dsw2026Tpi.CrossCutting.Exceptions;

public class AuthorizationException : AppException
{
    public AuthorizationException()
        : base(ErrorCodes.AUTHORIZATION_FAILED, nameof(ErrorCodes.AUTHORIZATION_FAILED))
    {
    }
}