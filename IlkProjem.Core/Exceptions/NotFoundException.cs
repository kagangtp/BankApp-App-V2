namespace IlkProjem.Core.Exceptions;

public class NotFoundException : Exception
{
    public string MessageKey { get; }

    public NotFoundException(string messageKey)
        : base(messageKey)
    {
        MessageKey = messageKey;
    }

    public NotFoundException(string messageKey, string message)
        : base(message)
    {
        MessageKey = messageKey;
    }
}
