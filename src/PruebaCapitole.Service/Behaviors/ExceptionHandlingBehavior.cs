using MediatR;
using PruebaCapitole.Domain.Common;
using PruebaCapitole.Service.Common;

namespace PruebaCapitole.Service.Behaviors
{
    /// <summary>
    /// Decorator around every handler: a DomainException (business rule broken, not found...)
    /// becomes a failed Result instead of an unhandled exception (500).
    /// Unexpected exceptions are NOT caught: those are real server errors.
    /// </summary>
    public sealed class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IResult<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (DomainException ex)
            {
                return TResponse.Failure(new Error(ex.Code, ex.Message));
            }
        }
    }
}
