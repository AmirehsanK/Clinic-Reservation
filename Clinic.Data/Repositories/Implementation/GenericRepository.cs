using Clinic.Data.Context;
using Clinic.Data.Entities;
using Clinic.Data.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Data.Repositories.Implementation;

public class GenericRepository<TEntity> (AppDbContext context,DbSet<TEntity> dbSet): IGenericRepository<TEntity> where TEntity : BaseEntity
{
    
    public async Task<TEntity> GetEntityById(int id)
    {
        return await GetAllEntities().SingleAsync(d => d.Id == id);
    }

    public IQueryable<TEntity> GetAllEntities()
    {
        return dbSet.AsQueryable();
    }

    public async Task Create(TEntity entity)
    {
        entity.CreateDate = DateTime.Now;
        entity.LastUpdateDate = DateTime.Now;
        await dbSet.AddAsync(entity);
    }

    public async Task CreateRangeEntities(List<TEntity> entities)
    {
        var list = new List<TEntity>();
        foreach (var item in entities)
        {
            item.CreateDate = DateTime.Now;
            item.LastUpdateDate = DateTime.Now;
            list.Add(item);
        }
        await dbSet.AddRangeAsync(list);
    }

    public async Task Delete(int id)
    {
        var data = await GetEntityById(id);
        data.IsDeleted = true;
        Update(data);
    }

    public void DeleteRange(List<TEntity> entities)
    {
        var list = new List<TEntity>();
        foreach (var item in entities)
        {
            item.IsDeleted = true;
            item.LastUpdateDate = DateTime.Now;
            list.Add(item);
        }
        dbSet.UpdateRange(list);
    }

    public void Update(TEntity entity)
    {
        entity.LastUpdateDate = DateTime.Now;
        dbSet.Update(entity);
    }

    public async Task DeletePermanently(int id)
    {
        var data = await GetEntityById(id);
        dbSet.Remove(data);
    }

    public async Task SaveChanges()
    {
        await context.SaveChangesAsync();
    }

    public ValueTask DisposeAsync()
    {
        return context.DisposeAsync();
    }

}