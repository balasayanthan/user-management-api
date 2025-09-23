namespace Api.Filters
{
    public sealed class CorrelationIdMiddleware(RequestDelegate next)
    {
        public const string HeaderName = "X-Correlation-Id";
        public async Task Invoke(HttpContext ctx)
        {
            if (!ctx.Request.Headers.TryGetValue(HeaderName, out var cid) || string.IsNullOrWhiteSpace(cid))
            {
                cid = ctx.TraceIdentifier;
            }
            ctx.Response.Headers[HeaderName] = cid!;
            await next(ctx);
        }
    }
}
