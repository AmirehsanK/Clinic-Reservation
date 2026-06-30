using Clinic.Data.Entities;
using Clinic.Data.Repositories.Interface;

namespace Clinic.Data.Repositories.Implementation;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    

    public Task<TEntity> GetEntityById(int id)
    {
        throw new NotImplementedException();
    }

    public IQueryable<TEntity> GetAllEntities()
    {
        throw new NotImplementedException();
    }

    public Task Create(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task CreateRangeEntities(List<TEntity> entities)
    {
        throw new NotImplementedException();
    }

    public Task Delete(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task DeleteRange(List<TEntity> entities)
    {
        throw new NotImplementedException();
    }

    public void Update(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public void DeletePermanently(int id)
    {
        throw new NotImplementedException();
    }

    public Task SaveChanges()
    {
        throw new NotImplementedException();
    }
    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}