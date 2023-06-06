using abys_agrivet_backend.Interfaces;

namespace abys_agrivet_backend.Repository.BranchRepository;

public interface BranchRepository<T> where T : class, IBranches
{
    public Task<List<T>> GetAllBranchesAvailable();
    public Task<dynamic> FindHighestBranchID();
    public Task<dynamic> saveBranch(T entity);
    public Task<List<T>> BranchManagementFindAll();
    public Task<dynamic> GroupBranchFunctions(string func, T entity);
}