using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PruebaCapitole.Service.Behaviors
{
    /// <summary>
    /// Decorator that measures how long each command/query takes (validation + handler + save)
    /// and logs it. It is the outermost behavior, so it times everything.
    /// </summary>
    public sealed class RequestTimingBehavior<TRequest, TResponse>(
        ILogger<RequestTimingBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                return await next();
            }
            finally
            {
                stopwatch.Stop();
                logger.LogInformation("{Request} handled in {ElapsedMs} ms",
                    typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
