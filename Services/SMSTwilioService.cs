using Twilio.Types;

namespace abys_agrivet_backend.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

public class SMSTwilioService
{
    public dynamic SendSMSService(string body, string phoneNumber)
    {
        string accountSID = "ACb4f1d2037092a3be68e8167bb5bdc50c";
        string authToken = "23e4dbdb151c1764bb06b0b680bc9d43";
        
        TwilioClient.Init(accountSID, authToken);
        var messageOptions = new CreateMessageOptions(
            new PhoneNumber(phoneNumber));
        messageOptions.From = new PhoneNumber("+18588159721");
        messageOptions.Body = body;

        var message = MessageResource.Create(messageOptions);
        return true;
    }    
}