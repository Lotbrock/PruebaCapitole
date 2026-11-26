using System.Text.Json.Serialization;

namespace PruebaCapitole.Service.Bookings.Queries
{
    /// <summary>Campo por el que se ordena el listado.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BookingSortBy
    {
        Start,
        End,
        CreatedAt
    }
}
