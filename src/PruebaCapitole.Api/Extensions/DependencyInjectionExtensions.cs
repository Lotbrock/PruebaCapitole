using Microsoft.EntityFrameworkCore;
using PruebaCapitole.Infrastructure.Data;
using PruebaCapitole.Infrastructure.Repositories;
using PruebaCapitole.Service.Abstractions;
using PruebaCapitole.Service.Bookings.Commands;
using PruebaCapitole.Service.Bookings.Queries;

namespace PruebaCapitole.Api.Extensions
{

    public static class DependencyInjectionExtensions
    {
       public static IServiceCollection AddServiceLayer(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(CreateBookingCommand).Assembly));

            return services;
        }

        public static IServiceCollection AddInfrastructureLayer(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CapitoleDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CapitoleDbContext>());
            services.AddScoped<IBookingReadService, BookingReadService>();

            return services;
        }
    }
}
