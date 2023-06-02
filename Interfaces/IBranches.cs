namespace abys_agrivet_backend.Interfaces;

public interface IBranches
{
    int id { get; set; }
    int branch_id { get; set; }
    string branchName { get; set; }
    string branchKey { get; set; }
    string branchPath { get; set; }
    char branchStatus { get; set; }
    DateTime createdAt { get; set; }
    DateTime updatedAt { get; set; }
}