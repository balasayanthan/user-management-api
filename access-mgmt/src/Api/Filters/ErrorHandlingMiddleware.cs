using System.Net;
using System.Text.Json;

namespace Api.Filters
{
    public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            try { await next(context); }
            catch (KeyNotFoundException ex)
            {
                await Write(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await Write(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled");
                await Write(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
            }
        }

        static Task Write(HttpContext ctx, HttpStatusCode code, string message)
        {
            ctx.Response.StatusCode = (int)code;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
        }
    }
}
