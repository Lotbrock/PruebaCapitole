using Microsoft.AspNetCore.Mvc.Testing;
using PruebaCapitole.Shared.IntegrationEvents;
using RabbitMQ.Client;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace PruebaCapitole.Api.IntegrationTests;

/// <summary>
/// Starts real PostgreSQL and RabbitMQ containers (Docker is required) and a real API in memory.
/// It is created once and shared by all the tests of the class.
/// </summary>
public sealed class ApiFixture : IAsyncLifetime
{
    private const string RabbitUser = "capitole";
    private const string RabbitPassword = "capitole";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder()
        .WithImage("rabbitmq:4-management")
        .WithUsername(RabbitUser)
        .WithPassword(RabbitPassword)
        .Build();

    private WebApplicationFactory<Program>? _factory;

    public HttpClient Client { get; private set; } = null!;

    public string RabbitHost => _rabbit.Hostname;
    public int RabbitPort => _rabbit.GetMappedPublicPort(5672);
    public string RabbitUserName => RabbitUser;
    public string RabbitPasswordValue => RabbitPassword;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _rabbit.StartAsync());
        await CreateSinkQueueAsync();

        // Environment variables win over appsettings.json and are read before the app starts.
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("RabbitMq__Host", RabbitHost);
        Environment.SetEnvironmentVariable("RabbitMq__Port", RabbitPort.ToString());
        Environment.SetEnvironmentVariable("RabbitMq__User", RabbitUser);
        Environment.SetEnvironmentVariable("RabbitMq__Password", RabbitPassword);
        Environment.SetEnvironmentVariable("Outbox__PollingIntervalSeconds", "1");
        Environment.SetEnvironmentVariable("Seeding__Enabled", "false");

        // Development: the API applies the migrations on start. The Outbox publisher runs as a hosted service.
        _factory = new WebApplicationFactory<Program>();
        Client = _factory.CreateClient();
    }

    /// <summary>
    /// The Worker normally declares the queues. Here nobody does, and the Outbox publishes with "mandatory"
    /// (an unroutable message is an error and would block the next ones). This queue receives every event.
    /// </summary>
    private async Task CreateSinkQueueAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = RabbitHost,
            Port = RabbitPort,
            UserName = RabbitUser,
            Password = RabbitPassword
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(BookingEventsRouting.Exchange, ExchangeType.Topic, durable: true);
        await channel.QueueDeclareAsync("tests.sink", durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync("tests.sink", BookingEventsRouting.Exchange, "booking.#");
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (_factory is not null) await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
        await _rabbit.DisposeAsync();
    }
}
