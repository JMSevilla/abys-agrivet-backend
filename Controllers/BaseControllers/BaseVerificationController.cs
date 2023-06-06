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
      var result = await _repository.SMSVerificationDataManagement(entity, new()
      {
         email = email,
         phoneNumber = phoneNumber
      });
      return Ok(result);
   }

   [Route("check-verification-code/{code}/{email}"), HttpPost]
   public async Task<IActionResult> CheckSMSVerification([FromRoute] string code, [FromRoute] string email)
   {
      var result = await _repository.SMSCheckVerificationCode(code, email);
      return Ok(result);
   }

   [Route("sms-resend-verification/{type}/{email}"), HttpPost]
   public async Task<IActionResult> SMSResendVerification([FromRoute] string type, [FromRoute] string email)
   {
      var result = await _repository.SMSResendVerificationCode(type, email);
      return Ok(result);
   }
}