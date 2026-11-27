namespace PruebaCapitole.Api.Correlation
{
    /// <summary>
    /// Reads X-Correlation-Id (or creates one), stores it for the request,
    /// echoes it in the response and adds it to every log line.
    /// </summary>
    public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        public const string HeaderName = "X-Correlation-Id";
        public const string ItemKey = "CorrelationId";

        public async Task InvokeAsync(HttpContext context)
        {
            var id = context.Request.Headers[HeaderName].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(id) || id.Length > 100)
                id = Guid.NewGuid().ToString();

            context.Items[ItemKey] = id;
            context.Response.Headers[HeaderName] = id;

            using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = id }))
            {
                await next(context);
            }
        }
    }
}
