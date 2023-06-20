using abys_agrivet_backend.DB;
using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Repository.BranchRepository;
using Microsoft.EntityFrameworkCore;

namespace abys_agrivet_backend.BusinessLayer.Logics;

public class EntityFrameworkBranches<TEntity, TContext> : BranchRepository<TEntity>
where TEntity : class, IBranches
where TContext : APIDBContext
{
    private readonly TContext context;

    public EntityFrameworkBranches(TContext context)
    {
        this.context = context;
    }

    public async Task<List<TEntity>> GetAllBranchesAvailable()
    {
        var result = await context.Set<TEntity>().Where(x => x.branchStatus == Convert.ToChar("1")).ToListAsync();
        return result;
    }

    public async Task<dynamic> FindHighestBranchID()
    {
        int highestBranchID = await context.Set<TEntity>().MaxAsync(x => x.branch_id);
        return highestBranchID;
    }

    public async Task<dynamic> saveBranch(TEntity entity)
    {
        var checkBranchIfExists = await context.Set<TEntity>()
            .AnyAsync(x => x.branchName == entity.branchName || x.branchKey == entity.branchKey);
        if (checkBranchIfExists)
        {
            return 401;
        }
        else
        {
            entity.branchStatus = Convert.ToChar("0");
            await context.Set<TEntity>().AddAsync(entity);
            await context.SaveChangesAsync();
            return 200;
        }
    }

    public async Task<List<TEntity>> BranchManagementFindAll()
    {
        var result = await context.Set<TEntity>().ToListAsync();
        return result;
    }

    public async Task<dynamic> GroupBranchFunctions(string func, TEntity entity)
    {
        try
        {
            var entityUpdate = await context.Set<TEntity>().Where(x => x.id == entity.id).FirstOrDefaultAsync();
            var checkBranchIfAny = await context.Set<TEntity>()
                .AnyAsync(t => t.branchName == entity.branchName && t.branchKey == entity.branchKey);
            var entityToRemove = await context.Set<TEntity>().FindAsync(entity.id);
            switch (func)
            {
                case "modify":
                    if (checkBranchIfAny)
                    {
                        return "branch_exist";
                    }
                    else
                    {
                        entityUpdate.branchPath = entityUpdate.branchPath;
                        entityUpdate.branchName = entity.branchName;
                        entityUpdate.branchKey = entity.branchKey;
                        await context.SaveChangesAsync();
                        return 200;
                    }
                case "activate":
                    entityUpdate.branchStatus = Convert.ToChar("1");
                    await context.SaveChangesAsync();
                    return 200;
                case "deactivate":
                    entityUpdate.branchStatus = Convert.ToChar("0");
                    await context.SaveChangesAsync();
                    return 200;
                case "deletion":
                    if (entityToRemove != null)
                    {
                        context.Set<TEntity>().Remove(entityToRemove);
                        await context.SaveChangesAsync();
                        return 200;
                    }

                    return "something_went_wrong";
            }

            return "something_went_wrong";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<TEntity>> BranchExceptAllBranch()
    {
        return await context.Set<TEntity>().Where(x => x.branch_id != 6).ToListAsync();
    }
}