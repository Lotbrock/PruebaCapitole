using Microsoft.EntityFrameworkCore;
using PruebaCapitole.Infrastructure.Data;

namespace PruebaCapitole.Api.Extensions
{
    public static class MigrationExtensions
    {
        /// <summary>Aplica las migraciones de EF Core que falten. Si no falta ninguna, no hace nada.</summary>
        public static IApplicationBuilder ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CapitoleDbContext>();

            db.Database.Migrate();

            return app;
        }
    }
}
