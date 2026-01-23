using System.Net;
using LeadBoard.Shared.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace LeadBoardNet.API.Controllers;

public class ApiControllerBase : ControllerBase
{
    protected ActionResult<ApiResponse<T>> HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<T>.Success(result.Value!));

        var response = ApiResponse<string>.Fail(result.Error);

        return result.StatusCode switch
        {
            HttpStatusCode.NotFound => NotFound(response),
            HttpStatusCode.BadRequest => BadRequest(response),
            HttpStatusCode.Conflict => Conflict(response),
            HttpStatusCode.Unauthorized => Unauthorized(response),
            HttpStatusCode.Forbidden => StatusCode(403, response),
            _ => StatusCode((int)result.StatusCode, response)
        };
    }
    
    // Sobrecarga para Result sin valor (operaciones void)
    protected ActionResult HandleResult(Result<object> result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<object>.Success(null, "Operación exitosa"));

        var response = ApiResponse<string>.Fail(result.Error);

        return result.StatusCode switch
        {
            HttpStatusCode.NotFound => NotFound(response),
            HttpStatusCode.BadRequest => BadRequest(response),
            HttpStatusCode.Conflict => Conflict(response),
            HttpStatusCode.Unauthorized => Unauthorized(response),
            HttpStatusCode.Forbidden => StatusCode(403, response),
            _ => StatusCode((int)result.StatusCode, response)
        };
    }
}