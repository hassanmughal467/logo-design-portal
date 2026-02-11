namespace LogoDesignPortal.Application.Exceptions;

/// <summary>
/// Exception thrown when a user is authenticated but doesn't have permission to access a resource.
/// This should result in a 403 Forbidden response, not 401 Unauthorized.
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message) : base(message)
    {
    }

    public ForbiddenAccessException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
