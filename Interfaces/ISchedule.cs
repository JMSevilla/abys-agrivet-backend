namespace abys_agrivet_backend.Interfaces;

public interface ISchedule
{
    int id { get; set; }
    int userid { get; set; }
    int branch { get; set; }
    string title { get; set; }
    string mockSchedule { get; set; }
    int? status { get; set; }
    int isHoliday { get; set; }
    string schedTime { get; set; }
    DateTime start { get; set; }
    DateTime end { get; set; }
    DateTime createdAt { get; set; }
    DateTime updatedAt { get; set; }
}