namespace abys_agrivet_backend.Helper.Schedule;

public class ScheduleHelper
{
    public int id { get; set; }
    public DateTime start { get; set; }
    public DateTime end { get; set; }
    public string title { get; set; }
    public bool isHoliday { get; set; }
}