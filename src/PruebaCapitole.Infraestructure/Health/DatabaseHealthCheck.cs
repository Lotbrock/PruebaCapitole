using Microsoft.Extensions.Diagnostics.HealthChecks;
using PruebaCapitole.Infrastructure.Data;

namespace PruebaCapitole.Infrastructure.Health
{
    /// <summary>Healthy if PostgreSQL accepts a connection.</summary>
    public sealed class DatabaseHealthCheck(CapitoleDbContext db) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                return await db.Database.CanConnectAsync(cancellationToken)
                    ? HealthCheckResult.Healthy()
                    : HealthCheckResult.Unhealthy("The database is not reachable.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("The database is not reachable.", ex);
            }
        }
    }
}
