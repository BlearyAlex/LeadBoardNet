using System.Net;

namespace LeadBoard.Shared.Wrappers;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string Error { get; }
    public HttpStatusCode StatusCode { get; }

    protected Result(T? value, bool success, string error, HttpStatusCode statusCode)
    {
        IsSuccess = success;
        Value = value;
        Error = error;
        StatusCode = statusCode;
    }

    // Success
    public static Result<T> Success(T value) => new(value, true, string.Empty, HttpStatusCode.OK);
    // Failure genérico
    public static Result<T> Failure(string error, HttpStatusCode code) => new(default, false, error, code);
    
    // Métodos helper específicos (OPCIONAL - pero muy útil)
    public static Result<T> NotFound(string error = "Recurso no encontrado")
        => new(default, false, error, HttpStatusCode.NotFound);

    public static Result<T> BadRequest(string error)
        => new(default, false, error, HttpStatusCode.BadRequest);

    public static Result<T> Conflict(string error)
        => new(default, false, error, HttpStatusCode.Conflict);

    public static Result<T> Unauthorized(string error = "No autorizado")
        => new(default, false, error, HttpStatusCode.Unauthorized);
}
