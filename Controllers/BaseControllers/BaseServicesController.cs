using abys_agrivet_backend.Authentication;
using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Repository.ServicesRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace abys_agrivet_backend.Controllers.BaseControllers;
[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(ApiKeyAuthFilter))]
public abstract class BaseServicesController<TEntity, TRepository> : ControllerBase
where TEntity : class, IServices
where TRepository : ServicesRepository<TEntity>
{
    private readonly TRepository _repository;
    public BaseServicesController(TRepository repository)
    {
        this._repository = repository;
    }

    [Route("create-new-services")]
    [HttpPost]
    public async Task<IActionResult> CreateNewServices(TEntity entity)
    {
        await _repository.CreateNewServices(entity);
        return Ok(200);
    }

    [Route("get-all-services"), HttpGet]
    public async Task<IActionResult> GetAllServices()
    {
        var result = await _repository.GetAllServices();
        return Ok(result);
    }

    [Route("delete-service/{id}"), HttpDelete]
    public async Task<IActionResult> DeleteService([FromRoute] int id)
    {
        var result = await _repository.DeleteService(id);
        return Ok(result);
    }

    [Route("activate-service/{id}/{type}"), HttpPut]
    [AllowAnonymous]
    public async Task<IActionResult> ActivateService([FromRoute] int id, [FromRoute] string type)
    {
        var result = await _repository.ChangeActivation(id, type);
        return Ok(result);
    }

    [Route("service-modification/{id}/{serviceName}"), HttpPut]
    [AllowAnonymous]
    public async Task<IActionResult> ServiceModification([FromRoute] int id, [FromRoute] string serviceName)
    {
        var result = await _repository.Modification(id, serviceName);
        return Ok(result);
    }
}