namespace PruebaCapitole.Infrastructure.Outbox
{
    public sealed class OutboxOptions
    {
        public const string SectionName = "Outbox";

        public int PollingIntervalSeconds { get; set; }
        public int BatchSize { get; set; }
    }
}
