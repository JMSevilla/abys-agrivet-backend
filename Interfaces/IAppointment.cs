namespace abys_agrivet_backend.Interfaces;

public interface IAppointment
{
    int id { get; set; }
    int scheduleId { get; set; }
    string email { get; set; }
    string phoneNumber { get; set; }
    string fullName { get; set; }
    int branch_id { get; set; }
    string service_id { get; set; }
    string petInfo { get; set; }
    string appointmentSchedule { get; set; }
    int status { get; set; }
    int isWalkedIn { get; set; }
    int? notify { get; set; }
    int reminderType { get; set; }
    int? isSessionStarted { get; set; }
    int? managersId { get; set; }
    DateTime created_at { get; set; }
    DateTime updated_at { get; set; }
    DateTime archive_indicator { get; set; }
}