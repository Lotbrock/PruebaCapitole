namespace PruebaCapitole.Service.Abstractions
{
    /// <summary>Port: gives the correlation id of the current request.</summary>
    public interface ICorrelationIdAccessor
    {
        string CorrelationId { get; }
    }
}
