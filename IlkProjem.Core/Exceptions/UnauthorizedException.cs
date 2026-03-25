namespace IlkProjem.Core.Exceptions;

public class UnauthorizedException : Exception
{
    public string MessageKey { get; }

    public UnauthorizedException(string messageKey)
        : base(messageKey)
    {
        MessageKey = messageKey;
    }

    public UnauthorizedException(string messageKey, string message)
        : base(message)
    {
        MessageKey = messageKey;
    }
}
