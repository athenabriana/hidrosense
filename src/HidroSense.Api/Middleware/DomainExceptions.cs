namespace HidroSense.Api.Middleware;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class DomainException : Exception
{
    public int StatusCode { get; }
    public DomainException(string message, int statusCode = StatusCodes.Status400BadRequest) : base(message)
        => StatusCode = statusCode;
}
