namespace abys_agrivet_backend.Interfaces;

public interface IFollowupAppointment
{
    int followupId { get; set; }
    int id { get; set; }
    string petInformation { get; set; }
    string title { get; set; }
    int branch_id { get; set; }
    string customerName { get; set; }
    string followupServices { get; set; }
    string? followupDescription { get; set; }
    string notificationType { get; set; }
    string? diagnosis { get; set; }
    string? treatment { get; set; }
    int status { get; set; }
    int isHoliday { get; set; }
    int? isSessionStarted { get; set; }
    int? managersId { get; set; }
    DateTime start { get; set; }
    DateTime end { get; set; }
}