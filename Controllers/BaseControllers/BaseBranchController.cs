using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Repository.BranchRepository;
using Microsoft.AspNetCore.Mvc;

namespace abys_agrivet_backend.Controllers.BaseControllers;
[Route("api/[controller]")]
[ApiController]
public class BaseBranchController<TEntity, TRepository> : ControllerBase
where TEntity : class, IBranches
where TRepository : BranchRepository<TEntity>
{
   private readonly TRepository _repository;

   public BaseBranchController(TRepository repository)
   {
      this._repository = repository;
   }

   [Route("get-all-branches"), HttpGet]
   public async Task<IActionResult> GetAllBranches()
   {
      return Ok(await _repository.GetAllBranchesAvailable());
   }
}