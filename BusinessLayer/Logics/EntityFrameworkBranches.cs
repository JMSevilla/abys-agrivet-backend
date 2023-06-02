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
}