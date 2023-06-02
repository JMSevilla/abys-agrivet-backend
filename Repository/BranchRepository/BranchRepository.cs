using abys_agrivet_backend.Interfaces;

namespace abys_agrivet_backend.Repository.BranchRepository;

public interface BranchRepository<T> where T : class, IBranches
{
    public Task<List<T>> GetAllBranchesAvailable();
}