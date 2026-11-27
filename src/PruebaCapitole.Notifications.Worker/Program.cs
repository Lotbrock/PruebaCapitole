using Microsoft.EntityFrameworkCore;
using PruebaCapitole.Notifications.Worker.Data;
using PruebaCapitole.Notifications.Worker.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<RetryOptions>(builder.Configuration.GetSection(RetryOptions.SectionName));

// The worker's own schema ("notifications") and its own migrations history table,
// so it never collides with the API's migrations in the same database.
builder.Services.AddDbContext<NotificationsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", NotificationsDbContext.Schema)));

builder.Services.AddScoped<NotificationRecorder>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHostedService<NotificationsConsumer>();
builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<NotificationsDbContext>().Database.MigrateAsync();
}

app.MapHealthChecks("/health");

app.Run();
