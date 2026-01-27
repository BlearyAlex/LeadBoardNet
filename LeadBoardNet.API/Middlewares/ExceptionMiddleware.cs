using System.Data.Common;
using System.Net;
using System.Text.Json;
using LeadBoard.Shared;
using LeadBoard.Shared.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace LeadBoardNet.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception: {Message}. Path: {Path}, Method: {Method}",
                ex.Message,
                context.Request.Path,
                context.Request.Method);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = MapException(ex);
        context.Response.StatusCode = (int)statusCode;

        var userMessage = _env.IsDevelopment()
            ? message
            : GetProductionMessage(statusCode);

        var response = ApiResponse<string>.Fail(
            userMessage,
            _env.IsDevelopment() ? new List<string> { ex.StackTrace ?? string.Empty } : null
        );

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, options));
    }

    private (HttpStatusCode statusCode, string message) MapException(Exception ex)
    {
        return ex switch
        {
            // Errores de Base de Datos
            DbUpdateException dbEx => (
                HttpStatusCode.Conflict,
                $"Error al actualizar la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}"
            ),
            DbException dbEx => (
                HttpStatusCode.ServiceUnavailable,
                $"Error de base de datos: {dbEx.Message}"
            ),

            // Errores de Timeout
            TimeoutException => (
                HttpStatusCode.RequestTimeout,
                "La operación excedió el tiempo límite"
            ),

            // Errores de HTTP Client (APIs externas)
            HttpRequestException httpEx => (
                HttpStatusCode.BadGateway,
                $"Error al comunicarse con servicio externo: {httpEx.Message}"
            ),

            // Errores de Operación Inválida (esto NO debería llegar aquí si usas Result)
            InvalidOperationException invEx => (
                HttpStatusCode.BadRequest,
                $"Operación inválida: {invEx.Message}"
            ),

            // Errores no autorizados (esto tampoco debería llegar si usas Result)
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "No tiene permisos para realizar esta acción"
            ),

            // Errores de subida de imagenes
            ImageUploadException imgEx => (
                imgEx.StatusCode,
                $"Error en la gestión de imágenes: {imgEx.Message}"
            ),

            // Catch-all para errores inesperados
            _ => (
                HttpStatusCode.InternalServerError,
                $"Error interno del servidor: {ex.Message}"
            )
        };
    }

    private static string GetProductionMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.ServiceUnavailable => "El servicio no está disponible temporalmente. Intente más tarde.",
            HttpStatusCode.RequestTimeout => "La operación tardó demasiado tiempo. Intente nuevamente.",
            HttpStatusCode.BadGateway => "Error al comunicarse con servicios externos.",
            HttpStatusCode.Conflict => "Conflicto al procesar la solicitud.",
            _ => "Ha ocurrido un error interno. Contacte al administrador."
        };
    }
}