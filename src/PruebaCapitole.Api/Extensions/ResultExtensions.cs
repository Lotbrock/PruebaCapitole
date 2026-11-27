using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PruebaCapitole.Api.Profiling;
using PruebaCapitole.Api.Responses;
using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Service.Common;

namespace PruebaCapitole.Api.Extensions
{
    /// <summary>
    /// Turns a Result into an HTTP response. Success and failure share the same envelope
    /// (ApiResponse) and both carry the elapsed time of the request.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>Builds the success envelope. The caller decides the status (Ok, CreatedAtAction...).</summary>
        public static ApiResponse<T> Envelope<T>(this ControllerBase controller, T? data) =>
            new(true, data, null, controller.HttpContext.ElapsedMs());

        public static ActionResult ToActionResult<T>(
            this ControllerBase controller, Result<T> result, Func<T, ActionResult> onSuccess) =>
            result.IsSuccess ? onSuccess(result.Value!) : ToProblem(controller, result.Error!);

        public static ActionResult ToActionResult(this ControllerBase controller, Result result) =>
            result.IsSuccess ? controller.Ok(controller.Envelope<object>(null)) : ToProblem(controller, result.Error!);

        private static ObjectResult ToProblem(ControllerBase controller, Error error)
        {
            var status = error.Code switch
            {
                Error.ValidationCode => StatusCodes.Status400BadRequest,
                DomainErrors.NotFound => StatusCodes.Status404NotFound,
                DomainErrors.Overlap or DomainErrors.InvalidTransition => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status422UnprocessableEntity
            };

            var body = new ApiResponse<object>(
                false, null, new ApiError(error.Code, error.Message, error.Details), controller.HttpContext.ElapsedMs());

            return new ObjectResult(body) { StatusCode = status };
        }

        /// <summary>
        /// Model binding errors (malformed JSON, invalid enum in the query...) also use the envelope,
        /// instead of the default ValidationProblemDetails.
        /// </summary>
        public static IMvcBuilder AddEnvelopeForModelErrors(this IMvcBuilder builder) =>
            builder.ConfigureApiBehaviorOptions(options =>
                options.InvalidModelStateResponseFactory = context =>
                {
                    var details = context.ModelState
                        .Where(entry => entry.Value?.Errors.Count > 0)
                        .ToDictionary(
                            entry => entry.Key,
                            entry => entry.Value!.Errors
                                .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "Valor no válido." : e.ErrorMessage)
                                .ToArray());

                    var body = new ApiResponse<object>(
                        false, null,
                        new ApiError(Error.ValidationCode, "Uno o más campos no son válidos.", details),
                        context.HttpContext.ElapsedMs());

                    return new ObjectResult(body) { StatusCode = StatusCodes.Status400BadRequest };
                });

        // The timing middleware stores the request stopwatch; here we read it (time until the response is built).
        public static double ElapsedMs(this HttpContext context) =>
            context.Items[RequestTimingMiddleware.StopwatchKey] is Stopwatch stopwatch
                ? Math.Round(stopwatch.Elapsed.TotalMilliseconds, 1)
                : 0;
    }
}
