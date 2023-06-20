using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using abys_agrivet_backend.Interfaces;

namespace abys_agrivet_backend.Model;
[Table("appointment")]
public class Appointment: IAppointment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    public string email { get; set; }
    public string phoneNumber { get; set; }
    public string fullName { get; set; }
    public int branch_id { get; set; }
    public string service_id { get; set; }
    public string petInfo { get; set; }
    public string appointmentSchedule { get; set; }
    public int status { get; set; }
    public int isWalkedIn { get; set; }
    public int? notify { get; set; }
    public int reminderType { get; set; }
    public int? isSessionStarted { get; set; }
    public int? managersId { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}