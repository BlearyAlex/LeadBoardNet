namespace LeadBoard.Shared.Wrappers;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T Value { get; set; }
    public string Error { get; set; }

    private Result(bool success, T value, string error)
        => (IsSuccess, Value, Error) = (success, value, error);

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}