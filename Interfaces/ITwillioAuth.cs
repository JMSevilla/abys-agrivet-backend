namespace abys_agrivet_backend.Interfaces;

public interface ITwillioAuth
{
    Guid id { get; set; }
    string accountSID { get; set; }
    string authtoken { get; set; }
    string identifier { get; set; }
}