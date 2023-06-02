using abys_agrivet_backend.DB;
using abys_agrivet_backend.Helper.VerificationCodeGenerator;
using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Repository.VerificationRepository;
using abys_agrivet_backend.Services;
using Microsoft.EntityFrameworkCore;

namespace abys_agrivet_backend.BusinessLayer.Logics;

public abstract class EntityFrameworkVerification<TEntity, TContext> : VerificationRepository<TEntity>
where TEntity : class, IVerification
where TContext : APIDBContext
{
    private readonly TContext context;
    public EntityFrameworkVerification(TContext context)
    {
        this.context = context;
    }
    public async Task<dynamic> SMSVerificationDataManagement(TEntity entity, VerificationParamsRequest verificationParamsRequest)
    {
        var checkSentCount = await context.Set<TEntity>().Where(x => x.email == entity.email).FirstOrDefaultAsync();
        var smsProvider = new SMSTwilioService();
        if (entity.type == "sms")
        {
            if (checkSentCount.resendCount >= 3)
            {
                return "max_sent_use_latest";
            }
            else
            {
                entity.code = GenerateVerificationCode.GenerateCode();
                entity.isValid = 1;
                entity.resendCount = checkSentCount.resendCount + 1;
                entity.createdAt = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
                entity.updatedAt = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
                smsProvider.SendSMSService(
                    ""+ entity.code + " " + "is your Abys-Agrivet Verification Code", verificationParamsRequest.phoneNumber
                );
                context.Set<TEntity>().Add(entity);
                await context.SaveChangesAsync();
                return entity;
            }
        }
        else
        {
            return "unavailable_service";
        }
    }
}