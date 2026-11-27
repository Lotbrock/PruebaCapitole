namespace PruebaCapitole.Api.Responses
{
    /// <summary>Error part of the envelope: stable code, readable message and, for validation, details per field.</summary>
    public sealed record ApiError(string Code, string Message, IReadOnlyDictionary<string, string[]>? Details);

    /// <summary>
    /// The ONLY shape the API returns, for success and for errors:
    /// { success, data, error, elapsedMs }. Null parts are omitted from the JSON.
    /// </summary>
    public sealed record ApiResponse<T>(bool Success, T? Data, ApiError? Error, double ElapsedMs);
}
