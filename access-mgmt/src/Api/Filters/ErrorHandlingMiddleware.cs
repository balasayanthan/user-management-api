using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using FluentValidation;

namespace Api.Filters
{
    public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            try { await next(context); }
            catch (ValidationException vex)
            {
                var pd = Create(context, StatusCodes.Status400BadRequest, "Validation failed", details: vex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
                await Write(context, pd);
            }
            catch (KeyNotFoundException kex)
            {
                var pd = Create(context, StatusCodes.Status404NotFound, kex.Message);
                await Write(context, pd);
            }
            catch (Application.Common.Exceptions.ConflictException cex)
            {
                var pd = Create(context, StatusCodes.Status409Conflict, cex.Message);
                await Write(context, pd);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbex)
            {
                // likely unique index violation, etc.
                var pd = Create(context, StatusCodes.Status409Conflict, "A conflicting change was detected.");
                logger.LogWarning(dbex, "DbUpdateException");
                await Write(context, pd);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled");
                var pd = Create(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
                await Write(context, pd);
            }
        }

        static ProblemDetails Create(HttpContext ctx, int status, string title, object? details = null)
        {
            return new ProblemDetails
            {
                Title = title,
                Status = status,
                Type = $"https://httpstatuses.com/{status}",
                Instance = ctx.Request.Path
            }.WithExtensions(new Dictionary<string, object?>
            {
                ["traceId"] = ctx.TraceIdentifier,
                ["details"] = details
            });
        }

        static Task Write(HttpContext ctx, ProblemDetails pd)
        {
            ctx.Response.StatusCode = pd.Status ?? (int)HttpStatusCode.InternalServerError;
            ctx.Response.ContentType = "application/problem+json";
            var json = JsonSerializer.Serialize(pd, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
            return ctx.Response.WriteAsync(json);
        }
    }

    file static class ProblemDetailsExtensions
    {
        public static ProblemDetails WithExtensions(this ProblemDetails pd, IDictionary<string, object?> ext)
        {
            foreach (var kv in ext) pd.Extensions[kv.Key] = kv.Value;
            return pd;
        }
    }
}
