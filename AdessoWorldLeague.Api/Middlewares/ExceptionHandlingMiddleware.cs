using FluentValidation;
using System.Net;
using System.Text.Json;

namespace AdessoWorldLeague.Api.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
            var response = new { errors };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (ArgumentException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var response = new { error = ex.Message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new { error = "Beklenmeyen bir hata oluştu." };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
