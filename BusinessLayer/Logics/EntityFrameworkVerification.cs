using abys_agrivet_backend.DB;
using abys_agrivet_backend.Helper.VerificationCodeGenerator;
using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Repository.VerificationRepository;
using abys_agrivet_backend.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using MailSettings = abys_agrivet_backend.Helper.MailSettings.MailSettings;

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
        var updateEntityVerification = await context.Set<TEntity>()
            .Where(x => x.email == entity.email && x.isValid == 1).FirstOrDefaultAsync();
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
                    string code = GenerateVerificationCode.GenerateCode();
                    updateEntityVerification.code = code;
                    updateEntityVerification.resendCount = updateEntityVerification.resendCount + 1;
                    updateEntityVerification.createdAt = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
                    updateEntityVerification.updatedAt = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
                    await context.SaveChangesAsync();
                    smsProvider.SendSMSService(
                        ""+ code + " " + "is your Abys-Agrivet Verification Code", "+63" + verificationParamsRequest.phoneNumber
                    );
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
            if (checkVerificationProfile)
            {
                if (checkSentCount.resendCount >= 3)
                {
                    return "max_sent_use_latest";
                }
                else
                {
                    updateEntityVerification.code = GenerateVerificationCode.GenerateCode();
                    updateEntityVerification.resendCount = updateEntityVerification.resendCount + 1;
                    updateEntityVerification.createdAt = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
                    updateEntityVerification.updatedAt = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
                    SendEmailSMTPWithCode(entity.email, updateEntityVerification.code,
                        "Kindly use this code for forgot password request.");
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

    public async Task<dynamic> ReminderSystem(int type, int id, string email, string phoneNumber)
    {
        var updateNotify = await context.Appointments.Where(x => x.id == id).FirstOrDefaultAsync();
        var smsProvider = new SMSTwilioService();
        if (type == 1)
        {
            updateNotify.notify = 1;
            SendWelcomeEmailSMTPWithoutCode(email,
                "We would like to remind you about your appointment tomorrow. Please go to the vet");
            await context.SaveChangesAsync();
            return 200;
        }
        else
        {
            updateNotify.notify = 1;
            smsProvider.SendSMSService(
                "We would like to remind you about your appointment tomorrow. Please go to the vet.", "+63" + phoneNumber
            );
            await context.SaveChangesAsync();
            return 200;
        }

        return 400;
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
            .Where(x => x.email == email).FirstOrDefaultAsync();
        
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
                smsProvider.SendSMSService(
                ""+ code + " " + "is your Abys-Agrivet Verification Code", "+63" + findUserMobileNumber.phoneNumber
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

    public async Task SendEmailSMTPWithCode(string email, string code, string? body)
    {
        /*string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\emailTemplate.html";
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
        smtp.Disconnect(true);*/
        var apiKey = "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";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("agrivetabys@gmail.com", "Abys Agrivet System");
        var subject = "Abys Agrivet Notification";
        var to = new EmailAddress(email, "User");
        var plainTextContent = "ABYS AGRIVET NOTIFICATIONS";
        var htmlContent = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>OTP Email</title>
</head>
<body>
    <div style=""text-align: center;"">
        <h1>Your OTP Code</h1>
        <p>Use the following code to verify your account:</p>
        <h2 style=""color: #007bff;"">" + code + @"</h2>
    </div>
</body>
</html>";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        await client.SendEmailAsync(msg);
    }

    public async Task SendWelcomeEmailSMTPWithoutCode(string email, string? body)
    {
        /*string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\welcomeTemplate.html";
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
        smtp.Disconnect(true);*/
        
        var apiKey = "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";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("agrivetabys@gmail.com", "Abys Agrivet System");
        var subject = "Abys Agrivet Notification";
        var to = new EmailAddress(email, "User");
        var plainTextContent = "ABYS AGRIVET NOTIFICATIONS";
        var htmlContent = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>OTP Email</title>
</head>
<body>
    <div style=""text-align: center;"">
        <h2 style=""color: #007bff;"">" + body + @"</h2>
    </div>
</body>
</html>";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        await client.SendEmailAsync(msg);
    }
}