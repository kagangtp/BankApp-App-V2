namespace IlkProjem.Core.Utilities.Results;

public class ErrorDataResult<T> : DataResult<T>
{
    // Returns default data (usually null) with a message
    public ErrorDataResult(T data, string message) : base(data, false, message)
    {
    }

    // Returns data without a message
    public ErrorDataResult(T data) : base(data, false)
    {
    }

    // Common usage: returns null with a message
    public ErrorDataResult(string message) : base(default!, false, message)
    {
    }

    // Common usage: returns null without a message
    public ErrorDataResult() : base(default!, false)
    {
    }
}