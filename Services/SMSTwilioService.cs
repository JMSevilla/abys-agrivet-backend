using Twilio.Types;

namespace abys_agrivet_backend.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

public class SMSTwilioService
{
    public dynamic SendSMSService(string body, string phoneNumber)
    {
        string accountSID = "ACe24560b7cbc7068fc0c67de0d729272a";
        string authToken = "3edc8f3d4a2ffc3275a77100fd54b041";
        
        TwilioClient.Init(accountSID, authToken);
        var messageOptions = new CreateMessageOptions(
            new PhoneNumber(phoneNumber));
        messageOptions.From = new PhoneNumber("+13203356710");
        messageOptions.Body = body;

        var message = MessageResource.Create(messageOptions);
        return true;
    }    
}