using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using abys_agrivet_backend.Interfaces;

namespace abys_agrivet_backend.Model;
[Table("schedule")]
public class Schedule : ISchedule
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    public int userid { get; set; }
    public int branch { get; set; }
    public string title { get; set; }
    public string mockSchedule { get; set; }
    public int? status { get; set; }
    public int isHoliday { get; set; }
    public DateTime start { get; set; }
    public DateTime end { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}