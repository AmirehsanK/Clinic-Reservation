using Clinic.Data.Entities;

namespace Clinic.Data.Repositories.Interface;

public interface IGenericRepository<TEntity> : IAsyncDisposable where TEntity : BaseEntity
{
    Task<TEntity> GetEntityById(int id);
    
    IQueryable<TEntity> GetAllEntities();

    Task Create(TEntity entity); 
    
    Task CreateRangeEntities(List<TEntity> entities);
    
    Task Delete(TEntity entity);
    
    Task DeleteRange(List<TEntity> entities);
    
    void Update(TEntity entity);
    
    void DeletePermanently(int id);
    
    Task SaveChanges();
}