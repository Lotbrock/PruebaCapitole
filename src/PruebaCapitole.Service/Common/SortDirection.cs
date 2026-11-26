using System.Text.Json.Serialization;

namespace PruebaCapitole.Service.Common
{
    /// <summary>Dirección del orden: ascendente o descendente.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SortDirection
    {
        Asc,
        Desc
    }
}
