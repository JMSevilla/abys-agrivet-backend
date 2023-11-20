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

    public async Task<dynamic> DeleteService(int id)
    {
        var deleteEntity = await context.Set<TEntity>().Where(x => x.id == id).FirstOrDefaultAsync();
        if (deleteEntity != null)
        {
            context.Set<TEntity>().Remove(deleteEntity);
            await context.SaveChangesAsync();
            return 200;
        }

        return 401;
    }

    public async Task<dynamic> ChangeActivation(int id, string type)
    {
        var result = await context.ServicesEnumerable.Where(x => x.id == id)
            .FirstOrDefaultAsync();
        if (result != null)
        {
            result.serviceStatus = type == "activate" ? 1 : 0;
            await context.SaveChangesAsync();
            return 200;
        }

        return 400;
    }

    public async Task<dynamic> Modification(int id, string serviceName)
    {
        var result = await context.ServicesEnumerable.Where(x => x.id == id)
            .FirstOrDefaultAsync();
        if (result != null)
        {
            result.serviceName = serviceName;
            await context.SaveChangesAsync();
            return 200;
        }

        return 400;
    }
}