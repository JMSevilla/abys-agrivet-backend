using abys_agrivet_backend.Helper;
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

   [Route("send-verification-code-sms"), HttpPost]
   public async Task<IActionResult> SendSMSVerification([FromBody] VerificationHelper entity)
   {
      var result = await _repository.SMSVerificationDataManagement(entity);
      return Ok(result);
   }

   [Route("check-verification-code/{code}/{email}/{type}"), HttpPost]
   public async Task<IActionResult> CheckSMSVerification([FromRoute] string code, [FromRoute] string email, [FromRoute] string? type)
   {
      var result = await _repository.SMSCheckVerificationCode(code, email, type);
      return Ok(result);
   }

   [Route("sms-resend-verification/{type}/{email}"), HttpPost]
   public async Task<IActionResult> SMSResendVerification([FromRoute] string type, [FromRoute] string email)
   {
      var result = await _repository.SMSResendVerificationCode(type, email);
      return Ok(result);
   }

   [Route("reminder-system/{type}/{id}/{email}/{phoneNumber}"), HttpPut]
   public async Task<IActionResult> ReminderSystem([FromRoute] int type, [FromRoute] int id, [FromRoute] string email,
      [FromRoute] string phoneNumber)
   {
      var result = await _repository.ReminderSystem(
         type,
         id,
         email,
         phoneNumber
      );
      return Ok(result);
   }
}