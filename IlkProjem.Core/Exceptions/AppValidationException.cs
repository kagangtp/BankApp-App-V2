namespace IlkProjem.Core.Exceptions;

public class AppValidationException : Exception
{
    public string MessageKey { get; }
    public Dictionary<string, string>? ValidationErrors { get; }

    public AppValidationException(string messageKey)
        : base(messageKey)
    {
        MessageKey = messageKey;
    }

    public AppValidationException(string messageKey, Dictionary<string, string> validationErrors)
        : base(messageKey)
    {
        MessageKey = messageKey;
        ValidationErrors = validationErrors;
    }

    public AppValidationException(string messageKey, string message, Dictionary<string, string>? validationErrors = null)
        : base(message)
    {
        MessageKey = messageKey;
        ValidationErrors = validationErrors;
    }
}
