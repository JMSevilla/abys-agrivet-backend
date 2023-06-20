using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using abys_agrivet_backend.Interfaces;

namespace abys_agrivet_backend.Model;
[Table("follow_up_appointment")]
public class FollowUpAppointment : IFollowupAppointment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int followupId { get; set; }
    public int id { get; set; }
    [ForeignKey("id")]
    public Appointment? Appointment { get; set; }
    public string petInformation { get; set; }
    public string title { get; set; }
    public int branch_id { get; set; }
    public string customerName { get; set; }
    public string followupServices { get; set; }
    public string? followupDescription { get; set; }
    public string notificationType { get; set; }
    public string? diagnosis { get; set; }
    public string? treatment { get; set; }
    public int status { get; set; }
    public int isHoliday { get; set; }
    public int? isSessionStarted { get; set; }
    public int? managersId { get; set; }
    public DateTime start { get; set; }
    public DateTime end { get; set; }
}