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
        var checkVerificationProfile = await context.Set<TEntity>().AnyAsync(x => x.email == entity.email);
        var smsProvider = new SMSTwilioService();
        if (entity.type == "sms")
        {
            if (checkVerificationProfile)
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
                        ""+ entity.code + " " + "is your Abys-Agrivet Verification Code", "+63" + verificationParamsRequest.phoneNumber
                    );
                    context.Set<TEntity>().Add(entity);
                    await context.SaveChangesAsync();
                    return 200;
                }
            }
            else
            {
                entity.code = GenerateVerificationCode.GenerateCode();
                entity.isValid = 1;
                entity.resendCount = 1;
                entity.createdAt = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
                entity.updatedAt = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
                smsProvider.SendSMSService(
                    ""+ entity.code + " " + "is your Abys-Agrivet Verification Code", "+63" + verificationParamsRequest.phoneNumber
                );
                context.Set<TEntity>().Add(entity);
                await context.SaveChangesAsync();
                return 200;
            }
        }
        else
        {
            return "unavailable_service";
        }
    }

    public async Task<dynamic> SMSCheckVerificationCode(string code, string email)
    {
        var verifyCode =
            await context.Set<TEntity>().AnyAsync(x => x.code == code && x.email == email && x.isValid == 1);
        var findVerificationByRef = await context.Set<TEntity>()
            .Where(x => x.code == code && x.email == email && x.isValid == 1).FirstOrDefaultAsync();
        var findUserByEmail =
            await context.UsersEnumerable.AnyAsync(x => x.email == email && x.verified == Convert.ToChar("0"));
        var getUserDataByEmail = await context.UsersEnumerable
            .Where(x => x.email == email && x.verified == Convert.ToChar("0")).FirstOrDefaultAsync();
        if (verifyCode)
        {
            if (findUserByEmail)
            {
                findVerificationByRef.isValid = 0;
                getUserDataByEmail.verified = Convert.ToChar("1");
                await context.SaveChangesAsync();
                return 200;
            }
            else
            {
                return 403;
            }
        }
        else
        {
            return 402;
        }
    }

    public async Task<dynamic> SMSResendVerificationCode(string type, string email)
    {
        var smsProvider = new SMSTwilioService();
        var checkSentCount = await context.Set<TEntity>().Where(x => x.email == email && x.isValid == Convert.ToChar("1")).FirstOrDefaultAsync();
        var findUserMobileNumber =
            await context.UsersEnumerable.Where(x => x.email == checkSentCount.email).FirstOrDefaultAsync();
        if (type == "sms")
        {
            if (checkSentCount.resendCount >= 3)
            {
                return 401;
            }
            else
            {
                var code = GenerateVerificationCode.GenerateCode();
                smsProvider.SendSMSService(
                    ""+ code + " " + "is your Abys-Agrivet Verification Code", findUserMobileNumber.phoneNumber
                );
                checkSentCount.code = code;
                checkSentCount.resendCount = checkSentCount.resendCount + 1;
                await context.SaveChangesAsync();
                return 200;

            }
        }
        else
        {
            return "email_type";
        }
    }
}