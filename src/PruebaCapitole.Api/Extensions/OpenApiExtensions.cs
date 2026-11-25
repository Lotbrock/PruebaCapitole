using Scalar.AspNetCore;

namespace PruebaCapitole.Api.Extensions
{
    public static class OpenApiExtensions
    {
         public static IServiceCollection AddOpenApiDocs(this IServiceCollection services)
        {
            services.AddOpenApi();
            return services;
        }
        
        public static WebApplication MapOpenApiDocs(this WebApplication app)
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
            return app;
        }
    }
}
