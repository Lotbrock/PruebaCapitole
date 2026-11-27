using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PruebaCapitole.Api.Correlation;
using PruebaCapitole.Infrastructure.Data;
using PruebaCapitole.Infrastructure.Data.Interceptors;
using PruebaCapitole.Infrastructure.Messaging;
using PruebaCapitole.Infrastructure.Outbox;
using PruebaCapitole.Infrastructure.Data.Seeding;
using PruebaCapitole.Infrastructure.Repositories;
using PruebaCapitole.Service.Abstractions;
using PruebaCapitole.Service.Behaviors;
using PruebaCapitole.Service.Bookings.Commands;
using PruebaCapitole.Service.Bookings.Queries;
using PruebaCapitole.Service.Mappings;

namespace PruebaCapitole.Api.Extensions
{

    public static class DependencyInjectionExtensions
    {
       public static IServiceCollection AddServiceLayer(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(CreateBookingCommand).Assembly);
                cfg.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(typeof(CreateBookingCommand).Assembly);

            services.AddAutoMapper(_ => { }, typeof(BookingMappingProfile).Assembly);

            return services;
        }

        public static IServiceCollection AddInfrastructureLayer(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<DispatchDomainEventsInterceptor>();
            services.AddDbContext<CapitoleDbContext>((sp, options) =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                       .AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>()));

            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CapitoleDbContext>());
            services.AddScoped<IBookingReadService, BookingReadService>();
            
            services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
            services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
            services.AddScoped<IOutbox, EfOutbox>();
            services.AddHostedService<OutboxPublisher>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICorrelationIdAccessor, HttpCorrelationIdAccessor>();

            // Seed with Bogus
            services.Configure<SeedingOptions>(configuration.GetSection(SeedingOptions.SectionName));
            services.AddSingleton(TimeProvider.System);
            services.AddScoped<BookingSeeder>();

            return services;
        }
    }
}
