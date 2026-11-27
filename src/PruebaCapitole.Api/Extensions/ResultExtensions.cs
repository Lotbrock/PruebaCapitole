using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Service.Common;

namespace PruebaCapitole.Api.Extensions
{
    /// <summary>
    /// Turns a Result into an HTTP response: the value if it succeeded,
    /// or a ProblemDetails with the error code and message if it failed.
    /// </summary>
    public static class ResultExtensions
    {
        public static ActionResult ToActionResult<T>(
            this ControllerBase controller, Result<T> result, Func<T, ActionResult> onSuccess) =>
            result.IsSuccess ? onSuccess(result.Value!) : ToProblem(result.Error!);

        public static ActionResult ToActionResult(this ControllerBase controller, Result result) =>
            result.IsSuccess ? controller.Ok() : ToProblem(result.Error!);

        private static ObjectResult ToProblem(Error error)
        {
            var status = error.Code switch
            {
                Error.ValidationCode => StatusCodes.Status400BadRequest,
                DomainErrors.NotFound => StatusCodes.Status404NotFound,
                DomainErrors.Overlap or DomainErrors.InvalidTransition => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status422UnprocessableEntity
            };

            var problem = new ProblemDetails
            {
                Status = status,
                Title = ReasonPhrases.GetReasonPhrase(status),
                Detail = error.Message
            };
            problem.Extensions["code"] = error.Code;
            if (error.Details is not null)
                problem.Extensions["errors"] = error.Details;

            return new ObjectResult(problem) { StatusCode = status };
        }
    }
}
