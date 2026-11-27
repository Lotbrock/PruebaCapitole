using PruebaCapitole.Service.Abstractions;

namespace PruebaCapitole.Api.Correlation
{
    /// <summary>Adapter of ICorrelationIdAccessor: reads the id the middleware stored for this request.</summary>
    public sealed class HttpCorrelationIdAccessor(IHttpContextAccessor httpContextAccessor) : ICorrelationIdAccessor
    {
        public string CorrelationId =>
            httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.ItemKey] as string
            ?? Guid.NewGuid().ToString(); // no HTTP request (e.g. seeding)
    }
}
