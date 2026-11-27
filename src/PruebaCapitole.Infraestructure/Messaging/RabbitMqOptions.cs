using RabbitMQ.Client;

namespace PruebaCapitole.Infrastructure.Messaging
{
    public sealed class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string User { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string VirtualHost { get; set; } = null!;

        /// <summary>Builds the RabbitMQ connection factory (used by the publisher and the health check).</summary>
        public ConnectionFactory ToConnectionFactory() => new()
        {
            HostName = Host,
            Port = Port,
            UserName = User,
            Password = Password,
            VirtualHost = VirtualHost
        };
    }
}
