namespace IlkProjem.Core.Exceptions;

public class ForbiddenAccessException : Exception
{
    public string MessageKey { get; }

    public ForbiddenAccessException(string messageKey)
        : base(messageKey)
    {
        MessageKey = messageKey;
    }

    public ForbiddenAccessException(string messageKey, string message)
        : base(message)
    {
        MessageKey = messageKey;
    }
}
