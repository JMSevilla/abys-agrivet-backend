namespace abys_agrivet_backend.Interfaces;

public interface IServices
{
    int id { get; set; }
    string serviceName { get; set; }
    string serviceBranch { get; set; }
    int? serviceStatus { get; set; }
    DateTime createdAt { get; set; }
    DateTime updatedAt { get; set; }
}