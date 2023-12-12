using Twilio.Types;

namespace abys_agrivet_backend.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

public class SMSTwilioService
{
    public dynamic SendSMSService(string body, string phoneNumber, string accountsid, string authtoken)
    {
        string accountSID = accountsid;
        string authToken = authtoken;
        
        TwilioClient.Init(accountSID, authToken);
        var messageOptions = new CreateMessageOptions(
            new PhoneNumber(phoneNumber));
        messageOptions.From = new PhoneNumber("+18588159721");
        messageOptions.Body = body;

        var message = MessageResource.Create(messageOptions);
        return true;
    }    
}