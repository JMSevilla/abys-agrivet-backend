using abys_agrivet_backend.Helper.SearchEngine;
using abys_agrivet_backend.Helper.SessionActions;
using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Model;

namespace abys_agrivet_backend.Repository.Appointment;

public interface AppointmentRepository<T> where T : class, IAppointment
{
    public Task<T> makeAnAppointment(T entity);
    public Task<dynamic> createSchedule(Schedule schedule);

    public Task<dynamic> GetAllSchedulePerBranch(int branch);
    public Task<dynamic> RemoveSelectedSchedule(int id);
    public Task<dynamic> getHighestID();
    public Task<dynamic> checkBeforeRemoving(int removeId);
    public Task<dynamic> NotifyBeforeExactDate();
    public Task<dynamic> CheckAffectedSchedules(string start, string end);
    public Task<dynamic> PostNewHoliday(Schedule schedule);
    public Task<dynamic> checkStartDateIfHoliday(int id);
    public Task<dynamic> removeAffectedSchedules(int id, int userid);
    public Task<dynamic> getAllAppointmentPerBranch(int branch_id);
    public Task<dynamic> createFollowUpAppointment(FollowUpAppointment followUpAppointment);
    public Task<dynamic> checkIfAppointmentIsDone();
    public Task<dynamic> AppointmentSession(SessionActions sessionActions);
    public Task<dynamic> GetAssignedSessionUsers(int manageruid);
    public Task<dynamic> AppointmentMakeItDone(int id);
    public Task<dynamic> FollowUpAppointmentsList(int branch_id);
    public IQueryable<dynamic> SearchFollowUpAppointments(string start, string end, string customerName);
}