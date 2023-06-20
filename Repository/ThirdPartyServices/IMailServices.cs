using abys_agrivet_backend.Helper.MailSettings;

namespace abys_agrivet_backend.Repository.ThirdPartyServices;

public interface IMailServices
{
    Task SendEmailAsync(MailRequest mailRequest);
}