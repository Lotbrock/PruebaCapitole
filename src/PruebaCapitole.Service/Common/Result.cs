namespace PruebaCapitole.Service.Common
{
    /// <summary>What went wrong: a stable code, a human message and (for validation) the details per field.</summary>
    public sealed record Error(string Code, string Message, IReadOnlyDictionary<string, string[]>? Details = null)
    {
        public const string ValidationCode = "validation.failed";
    }

    /// <summary>Lets the behaviors build a failed result for any request without reflection.</summary>
    public interface IResult<TSelf> where TSelf : IResult<TSelf>
    {
        static abstract TSelf Failure(Error error);
    }

    /// <summary>Response of a request that returns nothing: either success or an Error.</summary>
    public sealed class Result : IResult<Result>
    {
        private Result(Error? error) => Error = error;

        public Error? Error { get; }
        public bool IsSuccess => Error is null;

        public static Result Success() => new(null);
        public static Result Failure(Error error) => new(error);
    }

    /// <summary>Response of a request that returns a value: either the Value or an Error.</summary>
    public sealed class Result<T> : IResult<Result<T>>
    {
        private Result(T? value, Error? error)
        {
            Value = value;
            Error = error;
        }

        public T? Value { get; }
        public Error? Error { get; }
        public bool IsSuccess => Error is null;

        public static Result<T> Success(T value) => new(value, null);
        public static Result<T> Failure(Error error) => new(default, error);
    }
}
