using Clinic.Data.Context;
using Clinic.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Data.Repositories;

public class GenericRepository<TEntity> (AppDbContext context,DbSet<TEntity> dbSet): IGenericRepository<TEntity> where TEntity : BaseEntity
{
    
    public async Task<TEntity> GetEntityById(int id)
    {
        // SingleOrDefault, not Single: an id that does not exist (or has been
        // soft-deleted) is a normal "not found" for callers to handle, not a
        // reason to throw and surface a 500.
        return await GetAllEntities().SingleOrDefaultAsync(d => d.Id == id);
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

    public async Task<bool> Delete(int id)
    {
        var data = await GetEntityById(id);
        if (data == null)
        {
            return false;
        }
        data.IsDeleted = true;
        Update(data);
        return true;
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

    public async Task<bool> DeletePermanently(int id)
    {
        var data = await GetEntityById(id);
        if (data == null)
        {
            return false;
        }
        dbSet.Remove(data);
        return true;
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