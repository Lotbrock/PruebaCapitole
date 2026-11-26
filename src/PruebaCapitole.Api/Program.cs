using PruebaCapitole.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApiDocs();
builder.Services.AddServiceLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApiDocs();
    app.ApplyMigrations(); // crea/actualiza las tablas al arrancar (solo en Development)
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
