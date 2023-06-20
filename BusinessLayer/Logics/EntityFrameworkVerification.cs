using abys_agrivet_backend.DB;
using abys_agrivet_backend.Helper.MailSettings;
using abys_agrivet_backend.Helper.VerificationCodeGenerator;
using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Repository.VerificationRepository;
using abys_agrivet_backend.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;

namespace abys_agrivet_backend.BusinessLayer.Logics;

public abstract class EntityFrameworkVerification<TEntity, TContext> : VerificationRepository<TEntity>
where TEntity : class, IVerification
where TContext : APIDBContext
{
    private readonly TContext context;
    private readonly MailSettings _mailSettings;
    public EntityFrameworkVerification(TContext context, IOptions<MailSettings> mailSettings)
    {
        this.context = context;
        this._mailSettings = mailSettings.Value;
    }
    public async Task<dynamic> SMSVerificationDataManagement(TEntity entity, VerificationParamsRequest verificationParamsRequest)
    {
        var checkSentCount = await context.Set<TEntity>().Where(x => x.email == entity.email && x.isValid == 1).FirstOrDefaultAsync();
        var checkVerificationProfile = await context.Set<TEntity>().AnyAsync(x => x.email == entity.email && x.isValid == 1);
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
                    //smsProvider.SendSMSService(
                    //""+ entity.code + " " + "is your Abys-Agrivet Verification Code", "+63" + verificationParamsRequest.phoneNumber
                    //);
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
                 //smsProvider.SendSMSService(
                    //""+ entity.code + " " + "is your Abys-Agrivet Verification Code", "+63" + verificationParamsRequest.phoneNumber
                //);
                context.Set<TEntity>().Add(entity);
                await context.SaveChangesAsync();
                return 200;
            }
        }
        else
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
                    SendEmailSMTPWithCode(entity.email, entity.code,
                        "Kindly use this code for forgot password request.");
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
                SendEmailSMTPWithCode(entity.email, entity.code,
                    "Kindly use this code for forgot password request.");
                context.Set<TEntity>().Add(entity);
                await context.SaveChangesAsync();
                return 200;
            }
        }
    }

    public async Task<dynamic> SMSCheckVerificationCode(string code, string email, string? type = "account_activation")
    {
        var verifyCode =
            await context.Set<TEntity>().AnyAsync(x => x.code == code && x.email == email && x.isValid == 1);
        var findVerificationByRef = await context.Set<TEntity>()
            .Where(x => x.code == code && x.email == email && x.isValid == 1).FirstOrDefaultAsync();
        var findUserByEmail =
            await context.UsersEnumerable.AnyAsync(x => x.email == email && x.verified == Convert.ToChar("0"));
        var getUserDataByEmail = await context.UsersEnumerable
            .Where(x => x.email == email && x.verified == Convert.ToChar("0")).FirstOrDefaultAsync();
        
        var findfpuser = await context.UsersEnumerable.AnyAsync(x => x.email == email);
        if (verifyCode)
        {
            if (type == "account_activation")
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
                if (findfpuser)
                {
                    findVerificationByRef.isValid = 0;
                    await context.SaveChangesAsync();
                    return 200;
                }
                else
                {
                    return 403;
                }
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
                //smsProvider.SendSMSService(
                //""+ entity.code + " " + "is your Abys-Agrivet Verification Code", "+63" + verificationParamsRequest.phoneNumber
                //);
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

    public async Task SendEmailSMTPWithCode(string email, string code, string? body)
    {
        string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\emailTemplate.html";
        StreamReader str = new StreamReader(FilePath);
        string MailText = str.ReadToEnd();
        str.Close();
        MailText = MailText.Replace("[username]", "User").Replace("[email]", email).Replace("[verificationCode]", code)
            .Replace("[body]", body);
        var mail = new MimeMessage();
        mail.Sender = MailboxAddress.Parse(_mailSettings.Mail);
        mail.To.Add(MailboxAddress.Parse(email));
        mail.Subject = $"Welcome {email}";
        var builder = new BodyBuilder();
        builder.HtmlBody = MailText;
        mail.Body = builder.ToMessageBody();
        using var smtp = new SmtpClient();
        smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
        await smtp.SendAsync(mail);
        smtp.Disconnect(true);
    }

    public async Task SendWelcomeEmailSMTPWithoutCode(string email, string? body)
    {
        string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\welcomeTemplate.html";
        StreamReader str = new StreamReader(FilePath);
        string MailText = str.ReadToEnd();
        str.Close();
        MailText = MailText.Replace("[username]", "User").Replace("[email]", email)
            .Replace("[body]", body);
        var mail = new MimeMessage();
        mail.Sender = MailboxAddress.Parse(_mailSettings.Mail);
        mail.To.Add(MailboxAddress.Parse(email));
        mail.Subject = $"Welcome {email}";
        var builder = new BodyBuilder();
        builder.HtmlBody = MailText;
        mail.Body = builder.ToMessageBody();
        using var smtp = new SmtpClient();
        smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
        await smtp.SendAsync(mail);
        smtp.Disconnect(true);
    }
}