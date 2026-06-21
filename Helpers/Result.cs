namespace ZapretUI.Helpers;

public abstract class Result(string message)
{
    public string Message { get; set; } = message;
}

public sealed class Success(object? value, string message) : Result(message)
{
    public object? Value { get; set; } = value;
}
public sealed class Error(string message) : Result(message)
{
}