using abys_agrivet_backend.Authentication;
using abys_agrivet_backend.Helper.SearchEngine;
using abys_agrivet_backend.Helper.SessionActions;
using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Model;
using abys_agrivet_backend.Repository.Appointment;
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
        await _repository.makeAnAppointment(entity);
        return Ok(200);
    }

    [Route("create-schedule"), HttpPost]
    public async Task<IActionResult> createNewSchedule([FromBody] Schedule schedule)
    {
        var result = await _repository.createSchedule(schedule);
        return Ok(result);
    }

    [Route("get-all-schedule-per-branch/{branch}"), HttpGet]
    public async Task<IActionResult> GetAllSchedulePerBranch([FromRoute] int branch)
    {
        var result = await _repository.GetAllSchedulePerBranch(branch);
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

    [Route("appointment-make-it-done/{id}"), HttpPut]
    public async Task<IActionResult> MakeItDone([FromRoute] int id)
    {
        var result = await _repository.AppointmentMakeItDone(id);
        return Ok(result);
    }

    [Route("follow-up-appointments-list/{branch_id}"), HttpGet]
    public async Task<dynamic> FollowUpAppointmentsList([FromRoute] int branch_id)
    {
        var result = await _repository.FollowUpAppointmentsList(branch_id);
        return Ok(result);
    }

    [Route("search-follow-up-appointments/{start}/{end}/{customerName}"), HttpGet]
    public IActionResult SearchEngineFollowUpAppointments([FromRoute] string start, [FromRoute] string end, [FromRoute] string customerName)
    {
        var result = _repository.SearchFollowUpAppointments(start, end, customerName);
        return Ok(result);
    }
}