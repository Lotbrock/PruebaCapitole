namespace PruebaCapitole.Service.Abstractions
{
    /// <summary>Puerto: confirma todos los cambios de un caso de uso en un único commit.</summary>
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
