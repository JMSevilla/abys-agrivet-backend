using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Repository.VerificationRepository;
using Microsoft.AspNetCore.Mvc;

namespace abys_agrivet_backend.Controllers.BaseControllers;
[Route("api/[controller]")]
[ApiController]
public abstract class BaseVerificationController<TEntity, TRepository> : ControllerBase
where TEntity : class, IVerification
where TRepository : VerificationRepository<TEntity>
{
   private readonly TRepository _repository;

   public BaseVerificationController(TRepository repository)
   {
      this._repository = repository;
   }

   [Route("send-verification-code-sms/{email}/{phoneNumber}"), HttpPost]
   public async Task<IActionResult> SendSMSVerification([FromBody] TEntity entity, [FromRoute] string email,
      [FromRoute] string phoneNumber)
   {
      await _repository.SMSVerificationDataManagement(entity, new()
      {
         email = email,
         phoneNumber = phoneNumber
      });
      return Ok(200);
   }
}