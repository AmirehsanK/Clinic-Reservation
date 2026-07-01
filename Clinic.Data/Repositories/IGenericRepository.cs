using Clinic.Data.Entities;

namespace Clinic.Data.Repositories;

public interface IGenericRepository<TEntity> : IAsyncDisposable where TEntity : BaseEntity
{
    Task<TEntity> GetEntityById(int id);
    
    IQueryable<TEntity> GetAllEntities();

    Task Create(TEntity entity); 
    
    Task CreateRangeEntities(List<TEntity> entities);
    
    Task Delete(int id);
    
    void DeleteRange(List<TEntity> entities);
    
    void Update(TEntity entity);
    
    Task DeletePermanently(int id);
    
    Task SaveChanges();
}