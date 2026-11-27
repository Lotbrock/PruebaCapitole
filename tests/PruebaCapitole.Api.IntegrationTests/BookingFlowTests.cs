using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PruebaCapitole.Shared.IntegrationEvents;

namespace PruebaCapitole.Api.IntegrationTests;

/// <summary>
/// End-to-end tests against the real API, PostgreSQL and RabbitMQ.
/// Each test uses its own resource id, so the tests do not affect each other.
/// </summary>
public class BookingFlowTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static readonly TimeSpan EventTimeout = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan NoEventTimeout = TimeSpan.FromSeconds(3);

    private static readonly DateTimeOffset Day =
        new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero).AddDays(30);

    // ---------- helpers ----------

    private static DateTimeOffset At(double hours) => Day.AddHours(hours);

    private static string NewResource() => $"room-{Guid.NewGuid():N}";

    private Task<HttpResponseMessage> PostBookingAsync(string resource, double startHour, double endHour) =>
        api.Client.PostAsJsonAsync("/api/bookings", new
        {
            resourceId = resource,
            customerName = "Ana Pérez",
            customerEmail = "ana@example.com",
            start = At(startHour),
            end = At(endHour)
        });

    private async Task<Guid> CreateBookingAsync(string resource, double startHour, double endHour)
    {
        var response = await PostBookingAsync(resource, startHour, endHour);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadJsonAsync(response)).GetProperty("id").GetGuid();
    }

    private Task<HttpResponseMessage> ConfirmAsync(Guid id, string? correlationId = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/bookings/{id}/confirm");
        if (correlationId is not null) request.Headers.Add("X-Correlation-Id", correlationId);
        return api.Client.SendAsync(request);
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.Clone();
    }

    // ---------- tests ----------

    // CA1: valid data -> 201, Pending, appears in the list.
    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAndTheBookingIsPending()
    {
        // Arrange
        var resource = NewResource();

        // Act
        var response = await PostBookingAsync(resource, 10, 11);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var id = (await ReadJsonAsync(response)).GetProperty("id").GetGuid();
        var detail = await ReadJsonAsync(await api.Client.GetAsync($"/api/bookings/{id}"));
        Assert.Equal("Pending", detail.GetProperty("status").GetString());
    }

    [Fact]
    public async Task GetAll_FilteredByResource_ReturnsOnlyThatResourceBookings()
    {
        // Arrange
        var resource = NewResource();
        var id = await CreateBookingAsync(resource, 10, 11);
        await CreateBookingAsync(NewResource(), 10, 11); // another resource

        // Act
        var response = await api.Client.GetAsync($"/api/bookings?resourceId={resource}");

        // Assert
        var page = await ReadJsonAsync(response);
        Assert.Equal(1, page.GetProperty("totalCount").GetInt32());
        Assert.Equal(id, page.GetProperty("items")[0].GetProperty("id").GetGuid());
    }

    // CA2: confirming without overlap -> 200 and BookingConfirmedV1 is published.
    [Fact]
    public async Task Confirm_WithoutOverlap_ReturnsOk()
    {
        // Arrange
        var id = await CreateBookingAsync(NewResource(), 10, 11);

        // Act
        var response = await ConfirmAsync(id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Confirm_WithoutOverlap_PublishesBookingConfirmedV1WithTheCorrelationId()
    {
        // Arrange
        await using var listener = await EventListener.StartAsync(api, BookingEventsRouting.BookingConfirmedKey);
        var resource = NewResource();
        var id = await CreateBookingAsync(resource, 10, 11);
        var correlationId = $"test-{Guid.NewGuid():N}";

        // Act
        await ConfirmAsync(id, correlationId);

        // Assert
        var published = await listener.WaitForAsync<BookingConfirmedV1>(e => e.BookingId == id, EventTimeout);
        Assert.NotNull(published);
        Assert.Equal(resource, published.ResourceId);
        Assert.Equal("ana@example.com", published.CustomerEmail);
        Assert.Equal(correlationId, published.CorrelationId);
        Assert.NotEqual(Guid.Empty, published.EventId);
    }

    // CA3: overlap -> 409 booking.overlap and NO event is published.
    [Fact]
    public async Task Confirm_WithOverlappingConfirmedBooking_ReturnsConflictWithOverlapCode()
    {
        // Arrange
        var resource = NewResource();
        var first = await CreateBookingAsync(resource, 10, 11);
        await ConfirmAsync(first);
        var overlapping = await CreateBookingAsync(resource, 10.5, 11.5);

        // Act
        var response = await ConfirmAsync(overlapping);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("booking.overlap", (await ReadJsonAsync(response)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Confirm_WithOverlappingConfirmedBooking_PublishesNoEvent()
    {
        // Arrange
        await using var listener = await EventListener.StartAsync(api, BookingEventsRouting.BookingConfirmedKey);
        var resource = NewResource();
        var first = await CreateBookingAsync(resource, 10, 11);
        await ConfirmAsync(first);
        var overlapping = await CreateBookingAsync(resource, 10.5, 11.5);

        // Act
        await ConfirmAsync(overlapping);

        // Assert: the event of the first booking arrives, the rejected one never does.
        Assert.NotNull(await listener.WaitForAsync<BookingConfirmedV1>(e => e.BookingId == first, EventTimeout));
        Assert.Null(await listener.WaitForAsync<BookingConfirmedV1>(e => e.BookingId == overlapping, NoEventTimeout));
    }

    // CA4: adjacency (11:00-12:00 after 10:00-11:00) is not an overlap.
    [Fact]
    public async Task Confirm_WithAdjacentConfirmedBooking_ReturnsOk()
    {
        // Arrange
        var resource = NewResource();
        var first = await CreateBookingAsync(resource, 10, 11);
        await ConfirmAsync(first);
        var adjacent = await CreateBookingAsync(resource, 11, 12);

        // Act
        var response = await ConfirmAsync(adjacent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // CA5: cancelling an already cancelled booking -> 422 booking.invalid_transition.
    [Fact]
    public async Task Cancel_AlreadyCancelledBooking_ReturnsUnprocessableEntityWithInvalidTransitionCode()
    {
        // Arrange
        var id = await CreateBookingAsync(NewResource(), 10, 11);
        var cancelUrl = $"/api/bookings/{id}/cancel";
        await api.Client.PostAsJsonAsync(cancelUrl, new { reason = "Cambio de planes" });

        // Act
        var response = await api.Client.PostAsJsonAsync(cancelUrl, new { reason = "Otra vez" });

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("booking.invalid_transition", (await ReadJsonAsync(response)).GetProperty("code").GetString());
    }

    // CA6: start in the past or End <= Start -> 400 with the errors by field (ProblemDetails).
    [Fact]
    public async Task Create_WithStartInThePastAndEndBeforeStart_ReturnsBadRequestWithErrorsByField()
    {
        // Arrange
        var body = new
        {
            resourceId = NewResource(),
            customerName = "Ana Pérez",
            customerEmail = "ana@example.com",
            start = DateTimeOffset.UtcNow.AddDays(-1),
            end = DateTimeOffset.UtcNow.AddDays(-2)
        };

        // Act
        var response = await api.Client.PostAsJsonAsync("/api/bookings", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadJsonAsync(response);
        Assert.Equal("validation.failed", problem.GetProperty("code").GetString());
        var errors = problem.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Start", out _));
        Assert.True(errors.TryGetProperty("End", out _));
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNotFoundWithItsCode()
    {
        // Arrange
        var unknownId = Guid.NewGuid();

        // Act
        var response = await api.Client.GetAsync($"/api/bookings/{unknownId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("booking.not_found", (await ReadJsonAsync(response)).GetProperty("code").GetString());
    }
}
