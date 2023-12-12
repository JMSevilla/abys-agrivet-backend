namespace abys_agrivet_backend.Helper;

public class VerificationHelper
{
    public int id { get; set; }
    public string email { get; set; }
    public string code { get; set; }
    public int resendCount { get; set; }
    public int isValid { get; set; }
    public string? type { get; set; }
    public string phoneNumber { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}