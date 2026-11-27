using System.Text.Json.Serialization;
using PruebaCapitole.Api.Correlation;
using PruebaCapitole.Api.Extensions;
using PruebaCapitole.Api.Profiling;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)
    .AddEnvelopeForModelErrors();
builder.Services.AddOpenApiDocs();
builder.Services.AddServiceLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApiDocs();
    app.ApplyMigrations(); 
    await app.SeedDataAsync(); 
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
