using FluentValidation;
using MediatR;
using PruebaCapitole.Service.Common;

namespace PruebaCapitole.Service.Behaviors
{
    /// <summary>
    /// Decorator that runs the FluentValidation validators of the request before the handler.
    /// If any rule fails, the handler is not executed and a failed Result is returned.
    /// </summary>
    public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IResult<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var context = new ValidationContext<TRequest>(request);

            var failures = new List<FluentValidation.Results.ValidationFailure>();
            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(context, cancellationToken);
                failures.AddRange(result.Errors);
            }

            if (failures.Count == 0)
                return await next();

            var details = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

            return TResponse.Failure(new Error(Error.ValidationCode, "Uno o más campos no son válidos.", details));
        }
    }
}
