using abys_agrivet_backend.DB;
using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Repository.ServicesRepository;
using Microsoft.EntityFrameworkCore;

namespace abys_agrivet_backend.BusinessLayer.Logics;

public abstract class EntityFrameworkServices<TEntity, TContext> : ServicesRepository<TEntity>
where TEntity : class, IServices
where TContext : APIDBContext
{
    private readonly TContext context;

    public EntityFrameworkServices(TContext context)
    {
        this.context = context;
    }

    public async Task<TEntity> CreateNewServices(TEntity entity)
    {
        entity.serviceStatus = 1;
        await context.AddAsync(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<List<TEntity>> GetAllServices()
    {
        return await context.Set<TEntity>().ToListAsync();
    }
}