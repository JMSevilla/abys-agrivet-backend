using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using abys_agrivet_backend.Interfaces;

namespace abys_agrivet_backend.Model;
[Table("branches")]
public class Branch : IBranches
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    public int branch_id { get; set; }
    public string branchName { get; set; }
    public string branchKey { get; set; }
    public string branchPath { get; set; }
    public char branchStatus { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}