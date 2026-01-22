namespace LeadBoard.Shared.Wrappers;

public class ApiResponse<T>
{
    public bool Succeeded { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public List<string> Errors { get; set; }

    public static ApiResponse<T> Success(T data, string message = null)
        => new() { Succeeded = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, List<string> errors = null)
        => new() { Succeeded = false, Message = message, Errors = errors };
}