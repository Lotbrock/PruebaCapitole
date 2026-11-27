namespace PruebaCapitole.Service.Abstractions
{
    /// <summary>
    /// Port: stores an integration event to be published later.
    /// It does NOT save: the row is committed with the same SaveChanges as the business data.
    /// </summary>
    public interface IOutbox
    {
        void Add(string routingKey, object integrationEvent, string correlationId);
    }
}
