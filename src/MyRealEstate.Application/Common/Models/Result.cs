namespace MyRealEstate.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; private set; }
    public string Error { get; private set; } = string.Empty;
    
    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    
    public static Result<T> Success<T>(T value) => new(true, value, string.Empty);
    public static Result<T> Failure<T>(string error) => new(false, default!, error);
}

public class Result<T> : Result
{
    public T Value { get; private set; }
    
    internal Result(bool isSuccess, T value, string error) : base(isSuccess, error)
    {
        Value = value;
    }
}
