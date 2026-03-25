// Core/Exceptions/BusinessException.cs
using IlkProjem.Core.Enums;

namespace IlkProjem.Core.Exceptions;
public class BusinessException : Exception
{
    public BusinessErrorCode ErrorCode { get; }
    public string MessageKey { get; }

    public BusinessException(BusinessErrorCode errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
        MessageKey = errorCode.ToString();
    }

    public BusinessException(BusinessErrorCode errorCode, string messageKey, string message) : base(message)
    {
        ErrorCode = errorCode;
        MessageKey = messageKey;
    }
}