using PruebaCapitole.Infrastructure.Data.Seeding;

namespace PruebaCapitole.Api.Extensions
{
    public static class SeedExtensions
    {
        public static async Task<IApplicationBuilder> SeedDataAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<BookingSeeder>();

            await seeder.SeedAsync();

            return app;
        }
    }
}
