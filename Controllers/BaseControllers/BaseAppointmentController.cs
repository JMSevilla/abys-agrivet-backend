using abys_agrivet_backend.Authentication;
using abys_agrivet_backend.Helper.SearchEngine;
using abys_agrivet_backend.Helper.SessionActions;
using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Model;
using abys_agrivet_backend.Repository.Appointment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace abys_agrivet_backend.Controllers.BaseControllers;
[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(ApiKeyAuthFilter))]

public abstract class BaseAppointmentController<TEntity, TRepository> : ControllerBase
where TEntity : class, IAppointment
where TRepository : AppointmentRepository<TEntity>
{
    private readonly TRepository _repository;

    public BaseAppointmentController(TRepository repository)
    {
        this._repository = repository;
    }

    [Route("create-new-appointment"), HttpPost]
    public async Task<IActionResult> MakeAnAppointment([FromBody] TEntity entity)
    {
        var result = await _repository.makeAnAppointment(entity);
        return Ok(result);
    }

    [Route("create-schedule"), HttpPost]
    public async Task<IActionResult> createNewSchedule([FromBody] Schedule schedule)
    {
        var result = await _repository.createSchedule(schedule);
        return Ok(result);
    }

    [Route("get-all-schedule-per-branch/{branch}/{userid}"), HttpGet]
    public async Task<IActionResult> GetAllSchedulePerBranch([FromRoute] int branch, [FromRoute] int userid)
    {
        var result = await _repository.GetAllSchedulePerBranch(branch, userid);
        return Ok(result);
    }

    [Route("remove-selected-schedule/{id}"), HttpDelete]
    public async Task<IActionResult> RemoveSelected([FromRoute] int id)
    {
        var result = await _repository.RemoveSelectedSchedule(id);
        return Ok(result);
    }

    [Route("get-highest-id"), HttpGet]
    public async Task<IActionResult> getHighestID()
    {
        var result = await _repository.getHighestID();
        return Ok(result);
    }

    [Route("check-before-removing/{id}"), HttpGet]
    public async Task<IActionResult> checkBeforeRemoving([FromRoute] int id)
    {
        var result = await _repository.checkBeforeRemoving(id);
        return Ok(result);
    }

    [Route("check-reminder"), HttpGet]
    public async Task<IActionResult> checkAllReminders()
    {
        var result = await _repository.NotifyBeforeExactDate();
        return Ok(result);
    }

    [Route("check-affected-schedules/{start}/{end}"), HttpGet]
    public async Task<IActionResult> FindAffectedSchedules([FromRoute] string start, [FromRoute] string end)
    {
        var result = await _repository.CheckAffectedSchedules(start, end);
        return Ok(result);
    }

    [Route("check-holidays/{start}/{end}"), HttpGet]
    public async Task<IActionResult> FindHolidaysSchedules([FromRoute] string start, [FromRoute] string end)
    {
        var result = await _repository.CheckHolidaysSchedules(start, end);
        return Ok(result);
    }

    [Route("post-new-holiday"), HttpPost]
    public async Task<IActionResult> PostNewHoliday([FromBody] Schedule schedule)
    {
        var result = await _repository.PostNewHoliday(schedule);
        return Ok(result);
    }

    [Route("check-if-holiday/{id}"), HttpGet]
    public async Task<IActionResult> CheckIfStartIsHoliday([FromRoute] int id)
    {
        var result = await _repository.checkStartDateIfHoliday(id);
        return Ok(result);
    }

    [Route("remove-affected-schedules/{id}/{userid}"), HttpDelete]
    public async Task<IActionResult> removeAffectedSchedules([FromRoute] int id, [FromRoute] int userid)
    {
        var result = await _repository.removeAffectedSchedules(id, userid);
        return Ok(result);
    }

    [Route("get-all-appointments-per-branch/{branch_id}"), HttpGet]
    public async Task<IActionResult> getAppointmentPerBranch([FromRoute] int branch_id)
    {
        var result = await _repository.getAllAppointmentPerBranch(branch_id);
        return Ok(result);
    }

    [Route("create-follow-up-appointment"), HttpPost]
    public async Task<IActionResult> createNewFollowUpAppointment([FromBody] FollowUpAppointment followUpAppointment)
    {
        var result = await _repository.createFollowUpAppointment(followUpAppointment);
        return Ok(result);
    }

    [Route("check-appointment-if-done"), HttpGet]
    public async Task<IActionResult> CheckAppointmentIfDone()
    {
        var result = await _repository.checkIfAppointmentIsDone();
        return Ok(result);
    }

    [Route("appointment-session-actions"), HttpPut]
    public async Task<IActionResult> AppointmentActions([FromBody] SessionActions sessionActions)
    {
        var result = await _repository.AppointmentSession(sessionActions);
        return Ok(result);
    }

    [Route("get-assigned-user-session/{manageruid}"), HttpGet]
    public async Task<IActionResult> GetAssignedSessionUser([FromRoute] int manageruid)
    {
        var result = await _repository.GetAssignedSessionUsers(manageruid);
        return Ok(result);
    }

    [Route("appointment-make-it-done/{id}/{deletionId}"), HttpPut]
    public async Task<IActionResult> MakeItDone([FromRoute] int id, [FromRoute] int deletionId)
    {
        var result = await _repository.AppointmentMakeItDone(id, deletionId);
        return Ok(result);
    }

    [Route("follow-up-appointments-list/{branch_id}/{appointmentId}"), HttpGet]
    public async Task<dynamic> FollowUpAppointmentsList([FromRoute] int branch_id, [FromRoute] int appointmentId)
    {
        var result = await _repository.FollowUpAppointmentsList(branch_id, appointmentId);
        return Ok(result);
    }

    [Route("search-follow-up-appointments/{start}/{end}/{customerName}"), HttpGet]
    public IActionResult SearchEngineFollowUpAppointments([FromRoute] string start, [FromRoute] string end, [FromRoute] string customerName)
    {
        var result = _repository.SearchFollowUpAppointments(start, end, customerName);
        return Ok(result);
    }

    [Route("follow-up-session-management"), HttpPut]
    public async Task<IActionResult> FollowUpSessionManagement([FromBody] FollowUpSessionActions followUpSessionActions)
    {
        var result = await _repository.FollowUpAppointmentSession(followUpSessionActions);
        return Ok(result);
    }

    [Route("follow-up-count-done/{branch_id}/{appointmentId}"), HttpGet]
    public async Task<IActionResult> FollowUpCountDoneSession([FromRoute] int branch_id, [FromRoute] int appointmentId)
    {
        var result = await _repository.CountSessionDone(branch_id, appointmentId);
        return Ok(result);
    }

    [Route("get-todays-appointment/{branch_id}"), HttpGet]
    public async Task<IActionResult> getTodaysAppointment([FromRoute] int branch_id)
    {
        var result = await _repository.getTodaysAppointment(branch_id);
        return Ok(result);
    }

    [Route("bring-appointment-lobby"), HttpPost]
    public async Task<IActionResult> PostNewAppointmentToLobby([FromBody] Lobby lobby)
    {
        var result = await _repository.BringAppointmentToLobby(lobby);
        return Ok(result);
    }

    [Route("find-all-lobbies/{branch_id}"), HttpGet]
    public async Task<IActionResult> FindAllWalkedInLobbies([FromRoute] int branch_id)
    {
        var result = await _repository.FindAllLobbies(branch_id);
        return Ok(result);
    }

    [Route("remove-after-proceed-from-lobby/{id}"), HttpDelete]
    public async Task<IActionResult> RemoveAfterProceedFromLobby([FromRoute] int id)
    {
        var result = await _repository.DeleteWhenProceedFromLobby(id);
        return Ok(result);
    }

    [Route("count-reports/{branch_id}/{type}"), HttpGet]
    public async Task<IActionResult> getCountsOnDashboard([FromRoute] int branch_id, [FromRoute] string type)
    {
        var result = await _repository.countAppointments(branch_id, type);
        return Ok(result);
    }

    [Route("get-all-walked-in-appointments/{branch_id}"), HttpGet]
    public async Task<IActionResult> getAllWalkedInAppointments([FromRoute] int branch_id)
    {
        var result = await _repository.getAllWalkedInPerBranch(branch_id);
        return Ok(result);
    }

    [Route("get-all-record-done-appointment/{branch_id}"), HttpGet]
    public async Task<IActionResult> GetAllRecordManagementAppointmentDone([FromRoute] int branch_id)
    {
        var result = await _repository.FindRecordManagementPerBranch(branch_id);
        return Ok(result);
    }

    [Route("get-all-record-all-branch"), HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllRecordAllBranch()
    {
        var result = await _repository.GetAllAppointmentBranch();
        return Ok(result);
    }

    [Route("get-user-by-manager-id/{manager_id}"), HttpGet]
    public async Task<IActionResult> getUserByManagerId([FromRoute] int manager_id)
    {
        var result = await _repository.findUserByManagerId(manager_id);
        return Ok(result);
    }

    [Route("find-follow-ups-by-appointment-id/{id}"), HttpGet]
    public async Task<IActionResult> FindFollowUpsByAppointmentId([FromRoute] int id)
    {
        var result = await _repository.FindFollowUpsOnRecordManagement(id);
        return Ok(result);
    }

    [Route("find-primary-appointments/{id}"), HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> FindPrimaryAppointments([FromRoute] int id)
    {
        var result = await _repository.FindPrimaryAppointments(id);
        return Ok(result);
    }

    [Route("counts-admin-report/{type}"), HttpGet]
    public async Task<IActionResult> CountAdminReportsCards([FromRoute] string type)
    {
        var result = await _repository.CountAdminDashboardCountable(type);
        return Ok(result);
    }

    [Route("find-appointment-by-email/{email}"), HttpGet]
    public async Task<IActionResult> FindAppointmentsByEmail([FromRoute] string email)
    {
        var result = await _repository.FindAppointmentsByEmail(email);
        return Ok(result);
    }

    [Route("count-appointment-customer-card/{type}/{email}"), HttpGet]
    public async Task<IActionResult> CountAppointmentCustomerCard([FromRoute] string type, [FromRoute] string email)
    {
        var result = await _repository.countAppointmentsCardCustomer(type, email);
        return Ok(result);
    }

    [Route("check-event-db-saved/{id}"), HttpGet]
    public async Task<IActionResult> CheckSavedDBSaved([FromRoute] int id)
    {
        var result = await _repository.CheckSavedEventOnDB(id);
        return Ok(result);
    }

    [Route("cancel-appointment-lobby/{id}"), HttpDelete]
    public async Task<IActionResult> CancelAppointmentLobby([FromRoute] int id)
    {
        var result = await _repository.CancelAppointmentLobby(id);
        return Ok(result);
    }

    [Route("filter-records-by-branch/{branch_id}"), HttpGet]
    public async Task<IActionResult> FilterRecordsByBranch([FromRoute] int branch_id)
    {
        var result = await _repository.FilterRecordsByBranch(branch_id);
        return Ok(result);
    }

    [Route("push-to-archive/{id}"), HttpPut]
    public async Task<IActionResult> PushToArchive([FromRoute] int id)
    {
        var result = await _repository.UpdateStatusToArchiveAppointment(id);
        return Ok(result);
    }

    [Route("delete-records/{id}"), HttpDelete]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteRecords([FromRoute] int id)
    {
        var result = await _repository.DeleteRecords(id);
        return Ok(result);
    }
}