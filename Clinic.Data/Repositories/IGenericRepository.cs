using Clinic.Data.Entities;

namespace Clinic.Data.Repositories;

public interface IGenericRepository<TEntity> : IAsyncDisposable where TEntity : BaseEntity
{
    /// <summary>
    /// Returns the entity with the given id, or null when no such entity exists
    /// (including when it has been soft-deleted).
    /// </summary>
    Task<TEntity> GetEntityById(int id);

    IQueryable<TEntity> GetAllEntities();

    Task Create(TEntity entity);

    Task CreateRangeEntities(List<TEntity> entities);

    /// <summary>
    /// Soft-deletes the entity. Returns false when no such entity exists.
    /// </summary>
    Task<bool> Delete(int id);

    void DeleteRange(List<TEntity> entities);

    void Update(TEntity entity);

    /// <summary>
    /// Permanently removes the entity. Returns false when no such entity exists.
    /// </summary>
    Task<bool> DeletePermanently(int id);

    Task SaveChanges();
}
