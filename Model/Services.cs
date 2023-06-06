using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using abys_agrivet_backend.Interfaces;

namespace abys_agrivet_backend.Model;
[Table("services")]
public class Services : IServices
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    public string serviceName { get; set; }
    public string serviceBranch { get; set; }
    public int? serviceStatus { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}