using LeadBoardNet.API.Middlewares;

namespace LeadBoardNet.API.Extensions;

public static class AppExtensions
{
    public static void UseErrorHandlingMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
    }
}