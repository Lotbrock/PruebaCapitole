using PruebaCapitole.Service.Bookings.Commands;

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

        
    }
}
