using abys_agrivet_backend.Interfaces;

namespace abys_agrivet_backend.Repository.ServicesRepository;

public interface ServicesRepository<T> where T: class, IServices
{
    public Task<T> CreateNewServices(T entity);
    public Task<List<T>> GetAllServices();
    public Task<dynamic> DeleteService(int id);
}