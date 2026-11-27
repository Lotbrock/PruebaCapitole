namespace PruebaCapitole.Notifications.Worker.Messaging
{
    public sealed class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string User { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string VirtualHost { get; set; } = null!;
    }

    public sealed class RetryOptions
    {
        public const string SectionName = "Retry";

        public int MaxAttempts { get; set; }

        public double BaseDelaySeconds { get; set; }
    }
}
