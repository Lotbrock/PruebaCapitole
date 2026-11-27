using System.Diagnostics;
using System.Globalization;

namespace PruebaCapitole.Api.Profiling
{
    /// <summary>
    /// Measures the whole HTTP request, logs it and returns it in the Server-Timing header
    /// (browsers show it in the Network tab). The same stopwatch feeds "elapsedMs" in the body.
    /// </summary>
    public sealed class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        public const string StopwatchKey = "RequestStopwatch";

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            context.Items[StopwatchKey] = stopwatch; // read later to put the time in the response body

            // Headers must be written before the response starts.
            context.Response.OnStarting(() =>
            {
                var ms = stopwatch.Elapsed.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture);
                context.Response.Headers["Server-Timing"] = $"total;dur={ms}";
                return Task.CompletedTask;
            });

            try
            {
                await next(context);
            }
            finally
            {
                stopwatch.Stop();
                logger.LogInformation("{Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                    context.Request.Method, context.Request.Path, context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
