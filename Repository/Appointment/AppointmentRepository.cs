using System.Linq.Expressions;
using abys_agrivet_backend.Helper.SearchEngine;
using abys_agrivet_backend.Helper.SessionActions;
using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Model;

namespace abys_agrivet_backend.Repository.Appointment;

public interface AppointmentRepository<T> where T : class, IAppointment
{
    public Task<dynamic> makeAnAppointment(T entity);
    public Task<dynamic> createSchedule(Schedule schedule);

    public Task<dynamic> GetAllSchedulePerBranch(int branch, int? userid);
    public Task<dynamic> RemoveSelectedSchedule(int id);
    public Task<dynamic> getHighestID();
    public Task<dynamic> checkBeforeRemoving(int removeId);
    public Task<dynamic> NotifyBeforeExactDate();
    public Task<dynamic> CheckAffectedSchedules(string start, string end);
    public Task<dynamic> PostNewHoliday(Schedule schedule);
    public Task<dynamic> checkStartDateIfHoliday(int id);
    public Task<dynamic> removeAffectedSchedules(int id, int userid);
    public Task<dynamic> getAllAppointmentPerBranch(int branch_id);
    public Task<dynamic> getAllWalkedInPerBranch(int branch_id);
    public Task<dynamic> createFollowUpAppointment(FollowUpAppointment followUpAppointment);
    public Task<dynamic> checkIfAppointmentIsDone();
    public Task<dynamic> AppointmentSession(SessionActions sessionActions);
    public Task<dynamic> GetAssignedSessionUsers(int manageruid);
    public Task<dynamic> AppointmentMakeItDone(int id, int deletionId);
    public Task<dynamic> FollowUpAppointmentsList(int branch_id, int appointmentId);
    public IQueryable<dynamic> SearchFollowUpAppointments(string start, string end, string customerName);
    public Task<dynamic> FollowUpAppointmentSession(FollowUpSessionActions followUpSessionActions);
    public Task<dynamic> getTodaysAppointment(int branch_id);
    public Task<dynamic> CountSessionDone(int branch_id, int id);
    public Task<dynamic> BringAppointmentToLobby(Lobby lobby);
    public Task<List<Lobby>> FindAllLobbies(int branch_id);
    public Task<dynamic> DeleteWhenProceedFromLobby(int id);
    public Task<int> countAppointments(int branch_id, string type);
    public Task<dynamic> FindRecordManagementPerBranch(int branch_id);
    public Task<dynamic> GetAllAppointmentBranch();
    public Task<dynamic> findUserByManagerId(int manager_id);
    public Task<dynamic> FindFollowUpsOnRecordManagement(int id);
    public Task<dynamic> FindPrimaryAppointments(int id);
    public Task<int> CountAdminDashboardCountable(string type);
    public Task<dynamic> FindAppointmentsByEmail(string email);
    public Task<int> countAppointmentsCardCustomer(string type, string email);
    public Task<dynamic> CheckHolidaysSchedules(string start, string end);
    public Task<dynamic> CheckSavedEventOnDB(int id);
    public Task<dynamic> CancelAppointmentLobby(int id);

    public Task<List<T>> FilterRecordsByBranch(int branch_id);
    public Task<dynamic> UpdateStatusToArchiveAppointment(int id);
    public Task<dynamic> DeleteRecords(int id);
}