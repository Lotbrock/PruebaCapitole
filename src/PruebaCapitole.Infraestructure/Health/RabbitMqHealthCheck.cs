using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using PruebaCapitole.Infrastructure.Messaging;

namespace PruebaCapitole.Infrastructure.Health
{
    /// <summary>Healthy if RabbitMQ accepts a connection.</summary>
    public sealed class RabbitMqHealthCheck(IOptions<RabbitMqOptions> options) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var connection = await options.Value.ToConnectionFactory()
                    .CreateConnectionAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("RabbitMQ is not reachable.", ex);
            }
        }
    }
}
