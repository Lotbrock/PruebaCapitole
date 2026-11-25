namespace PruebaCapitole.Service.Common
{
    /// <summary>Forma de respuesta del enunciado: { items, page, pageSize, totalCount }.</summary>
    public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
}
