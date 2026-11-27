namespace PruebaCapitole.Api.Extensions
{
    /// <summary>CORS for the front end. The allowed origins come from appsettings ("Cors:AllowedOrigins").</summary>
    public static class CorsExtensions
    {
        public const string PolicyName = "Frontend";

        public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
        {
            var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

            services.AddCors(options => options.AddPolicy(PolicyName, policy => policy
                .WithOrigins(origins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                // Headers the front may need to read from the response.
                .WithExposedHeaders("Location", "X-Correlation-Id")));

            return services;
        }
    }
}
