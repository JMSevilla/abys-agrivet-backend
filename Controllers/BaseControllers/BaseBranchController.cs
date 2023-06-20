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

   [Route("find-highest-branch-id"), HttpGet]
   public async Task<IActionResult> GetHighestBranchID()
   {
      return Ok(await _repository.FindHighestBranchID());
   }

   [Route("save-new-branch"), HttpPost]
   public async Task<IActionResult> SaveNewBranch([FromBody] TEntity entity)
   {
      var result = await _repository.saveBranch(entity);
      return Ok(result);
   }
   [Route("find-all-branch-management"), HttpGet]
   public async Task<IActionResult> FindAllBranch()
   {
      var result = await _repository.BranchManagementFindAll();
      return Ok(result);
   }

   [Route("group-branch-actions/{func}"), HttpPost]
   public async Task<IActionResult> GroupBranchActions([FromRoute] string func, TEntity entity)
   {
      var result = await _repository.GroupBranchFunctions(func, entity);
      return Ok(result);
   }

   [Route("appointment-branch-list"), HttpGet]
   public async Task<IActionResult> AppointmentBranchList()
   {
      var result = await _repository.BranchExceptAllBranch();
      return Ok(result);
   }
}