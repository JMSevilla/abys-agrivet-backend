using System.Globalization;
using System.Linq.Expressions;
using abys_agrivet_backend.DB;
using abys_agrivet_backend.Helper.MailSettings;
using abys_agrivet_backend.Helper.Schedule;
using abys_agrivet_backend.Helper.SearchEngine;
using abys_agrivet_backend.Helper.SessionActions;
using abys_agrivet_backend.Helper.VerificationCodeGenerator;
using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Model;
using abys_agrivet_backend.Repository.Appointment;
using abys_agrivet_backend.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace abys_agrivet_backend.BusinessLayer.Logics;

public abstract class EntityFrameworkAppointment<TEntity, TContext> : AppointmentRepository<TEntity>
    where TEntity : class, IAppointment
    where TContext : APIDBContext
{
    private readonly TContext context;
    private readonly MailSettings _mailSettings;

    public EntityFrameworkAppointment(TContext context, IOptions<MailSettings> mailSettings)
    {
        this.context = context;
        this._mailSettings = mailSettings.Value;
    }

    public async Task SendAppointmentEmailSMTPTWithoutCode(string email, string body)
    {
        string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\welcomeTemplate.html";
        StreamReader str = new StreamReader(FilePath);
        string MailText = str.ReadToEnd();
        str.Close();
        MailText = MailText.Replace("[username]", "User").Replace("[email]", email)
            .Replace("[body]", body);
        var mail = new MimeMessage();
        mail.Sender = MailboxAddress.Parse(_mailSettings.Mail);
        mail.To.Add(MailboxAddress.Parse(email));
        mail.Subject = $"Welcome {email}";
        var builder = new BodyBuilder();
        builder.HtmlBody = MailText;
        mail.Body = builder.ToMessageBody();
        using var smtp = new SmtpClient();
        smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
        await smtp.SendAsync(mail);
        smtp.Disconnect(true);
    }

    class AppointmentEntity
    {
        public int id { get; set; }
    }

    class ScheduleEntity
    {
        public int id { get; set; }
    }

    public async Task<dynamic> AppointmentMakeItDone(int id, int deletionId)
    {
        var updateEntity = await context.Set<TEntity>().Where(x => x.id == id)
            .FirstOrDefaultAsync();
        AppointmentEntity[] entites = JsonSerializer.Deserialize<AppointmentEntity[]>(updateEntity.appointmentSchedule);
        foreach (AppointmentEntity entity in entites)
        {
            var schedule = await context.Schedules.Where(x => x.id == deletionId).FirstOrDefaultAsync();
            if (updateEntity != null)
            {
                schedule.status = 0;
                updateEntity.status = 2;
                updateEntity.archive_indicator = DateTime.Today;
                await context.SaveChangesAsync();
                return 200;
            }
        }

        return 201;
    }

    class GSched
    {
        public int id { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string title { get; set; }
        public bool isHoliday { get; set; }
    }

    public async Task<dynamic> BringAppointmentToLobby(Lobby lobby)
    {
        GSched[]? scheds = JsonSerializer.Deserialize<GSched[]>(lobby.appointmentSchedule);
        if (lobby != null)
        {
            var checkWalkedInCustomerIfHasAccount = await context.UsersEnumerable.AnyAsync(
                x => x.email == lobby.email);
            if (checkWalkedInCustomerIfHasAccount)
            {
                var checkWalkedInUserInformation = await context.UsersEnumerable.Where(x => x.email == lobby.email)
                    .FirstOrDefaultAsync();
                await context.Lobbies.AddAsync(lobby);
                await context.SaveChangesAsync();
                return 200;
            }
            else
            {

                await context.Lobbies.AddAsync(lobby);
                await context.SaveChangesAsync();
                return 200;
            }
        }

        return 201;
    }

    public async Task<List<Lobby>> FindAllLobbies(int branch_id)
    {
        var result = await context.Lobbies.Where(x => x.branch_id == branch_id && x.isWalkedIn == 1).ToListAsync();
        return result;
    }

    public async Task<dynamic> DeleteWhenProceedFromLobby(int id)
    {
        var result = await context.Lobbies.FindAsync(id);
        if (result != null)
        {
            context.Lobbies.Remove(result);
            await context.SaveChangesAsync();
            return 200;
        }

        return 405;
    }

    public async Task<dynamic> makeAnAppointment(TEntity entity)
    {
        var smsProvider = new SMSTwilioService();
        var code = GenerateVerificationCode.GenerateCode();
        if (entity != null)
        {
            var highestIdFromSchedule = await context.Schedules
                .OrderByDescending(x => x.id)
                .Select(x => x.id).FirstOrDefaultAsync();
            GSched[]? schedules = JsonSerializer.Deserialize<GSched[]>(entity.appointmentSchedule);
            if (entity.isWalkedIn == 1)
            {
                var checkWalkedInCustomerIfHasAccount = await context.UsersEnumerable.AnyAsync(
                    x => x.email == entity.email);
                if (checkWalkedInCustomerIfHasAccount)
                {
                    var checkWalkedInUserInformation = await context.UsersEnumerable.Where(x => x.email == entity.email)
                        .FirstOrDefaultAsync();
                    foreach (GSched schedule in schedules)
                    {
                        if (entity.reminderType == 1)
                        {
                            SendAppointmentEmailSMTPTWithoutCode(entity.email,
                                "Thank you for making an appointment we will notify you ahead of time");
                        }
                        else
                        {
                            var getApiKey = await context.TwillioAuths
                                .Where(x => x.identifier == "twilio-auth").FirstOrDefaultAsync();
                            smsProvider.SendSMSService(
                                "Thank you for making an appointment we will notify you ahead of time", "+63" + entity.phoneNumber, getApiKey.accountSID, getApiKey.authtoken
                            );
                        }

                        Schedule scheduleprops = new Schedule();
                        scheduleprops.userid = checkWalkedInUserInformation.id;
                        scheduleprops.branch = entity.branch_id;
                        scheduleprops.mockSchedule = entity.appointmentSchedule;
                        scheduleprops.status = 1;
                        scheduleprops.isHoliday = 0;
                        scheduleprops.start = Convert.ToDateTime(schedule.start).ToLocalTime();
                        scheduleprops.title = schedule.title;
                        scheduleprops.end = schedule.end.AddDays(1);
                        entity.created_at = Convert.ToDateTime(schedule.start).ToLocalTime();
                        entity.updated_at = schedule.end.AddDays(1);
                        entity.archive_indicator = DateTime.Today;
                        await context.Schedules.AddAsync(scheduleprops);
                        await context.Set<TEntity>().AddAsync(entity);
                        await context.SaveChangesAsync();
                        return 200;
                    }

                    return 200;
                }
                else
                {
                    foreach (GSched schedule in schedules)
                    {
                        if (entity.reminderType == 1)
                        {
                            SendAppointmentEmailSMTPTWithoutCode(entity.email,
                                "Thank you for making an appointment we will notify you ahead of time");
                        }
                        else
                        {
                            var getApiKey = await context.TwillioAuths
                                .Where(x => x.identifier == "twilio-auth").FirstOrDefaultAsync();
                            smsProvider.SendSMSService(
                                "Thank you for making an appointment we will notify you ahead of time", "+63" + entity.phoneNumber, getApiKey.accountSID, getApiKey.authtoken
                            );
                        }
                        Schedule scheduleprops = new Schedule();
                        scheduleprops.userid = 0;
                        scheduleprops.branch = entity.branch_id;
                        scheduleprops.mockSchedule = entity.appointmentSchedule;
                        scheduleprops.status = 1;
                        scheduleprops.isHoliday = 0;
                        scheduleprops.start = Convert.ToDateTime(schedule.start).ToLocalTime();
                        scheduleprops.title = schedule.title;
                        scheduleprops.end = schedule.end.AddDays(1);
                        entity.created_at = Convert.ToDateTime(schedule.start).ToLocalTime();
                        entity.updated_at = schedule.end.AddDays(1);
                        entity.archive_indicator = DateTime.Today;
                        await context.Schedules.AddAsync(scheduleprops);
                        await context.Set<TEntity>().AddAsync(entity);
                        await context.SaveChangesAsync();
                        return 200;
                    }

                    return 200;
                }
            }
            else
            {
                if (entity.reminderType == 1)
                {
                    /*email service from smtp*/
                    SendAppointmentEmailSMTPTWithoutCode(entity.email,
                        "Thank you for making an appointment we will notify you ahead of time");
                    entity.scheduleId = highestIdFromSchedule;
                    entity.created_at = entity.created_at.AddDays(1);
                    entity.updated_at = entity.updated_at.AddDays(1);
                    entity.archive_indicator = DateTime.Today;
                    await context.Set<TEntity>().AddAsync(entity);
                    await context.SaveChangesAsync();
                    return 200;
                }
                else
                {
                    var getApiKey = await context.TwillioAuths
                        .Where(x => x.identifier == "twilio-auth").FirstOrDefaultAsync();
                    smsProvider.SendSMSService(
                        "Thank you for making an appointment we will notify you ahead of time", "+63" + entity.phoneNumber, getApiKey.accountSID, getApiKey.authtoken
                    );
                    entity.scheduleId = highestIdFromSchedule;
                    entity.created_at = entity.created_at.AddDays(1);
                    entity.updated_at = entity.updated_at.AddDays(1);
                    entity.archive_indicator = DateTime.Today;
                    await context.Set<TEntity>().AddAsync(entity);
                    await context.SaveChangesAsync();
                    return 200;
                }
            }
        }
        else
        {
            return 200;
        }
    }

    public async Task<dynamic> createSchedule(Schedule schedule)
    {
        var checkDay = await context.Schedules.AnyAsync(x => x.start == schedule.start);
        if (checkDay)
        {
            return "already_scheduled";
        }
        else
        {
            schedule.start = Convert.ToDateTime(schedule.start).ToLocalTime();
            schedule.end = Convert.ToDateTime(schedule.end).ToLocalTime();
            await context.Schedules.AddAsync(schedule);
            await context.SaveChangesAsync();
            return 200;
        }
    }

    public async Task<dynamic> GetAllSchedulePerBranch(int branch, int? userid)
    {
        int[] holidays = new[] { 1, 2 };
        int[] statuses = new[] { 1, 0 };
        if (userid == 0)
        {
            var getAllSchedule = await context.Schedules.Where(x => x.branch == branch
                    || x.branch == 8)
                .Select(t => new
                {
                    t.id,
                    t.title,
                    t.start,
                    t.end,
                    isHoliday = t.isHoliday == 1 || t.isHoliday == 2 ? true : false
                }).ToListAsync();
            return getAllSchedule;
        }
        else
        {
            var foundBranch = await context.Branches.Where(x => x.branchKey == "all")
                .FirstOrDefaultAsync();
            var getAllSchedule = await context.Schedules.Where(x => (x.branch == branch
                                                                     && x.userid == userid) || x.branch == foundBranch.branch_id)
                .Select(t => new
                {
                    t.id,
                    t.title,
                    t.start,
                    t.end,
                    isHoliday = t.isHoliday == 1 || t.isHoliday == 2 ? true : false
                }).ToListAsync();
            return getAllSchedule;
        }
    }

    public async Task<dynamic> RemoveSelectedSchedule(int id)
    {
        var entityRemove = await context.Schedules.FindAsync(id);
        var entityAppointmentRemove = await context.Appointments.Where(x => x.scheduleId == id)
            .FirstOrDefaultAsync();
        if (entityRemove != null && entityAppointmentRemove != null)
        {
            context.Schedules.Remove(entityRemove);
            context.Appointments.Remove(entityAppointmentRemove);
            await context.SaveChangesAsync();
            return 200;
        }

        return 405;
    }

    public async Task<dynamic> getHighestID()
    {
        var highestId = await context.Schedules.OrderByDescending(e => e.id).Select(
            t => new
            {
                t.id
            }).FirstOrDefaultAsync();
        return highestId;
    }

    public async Task<dynamic> checkBeforeRemoving(int removeId)
    {
        var checkFromSchedules = await context.Schedules.AnyAsync(x => x.id == removeId);
        var checkFromAppointment = await context.Appointments.AnyAsync(x => x.scheduleId == removeId);
        if (checkFromSchedules && checkFromAppointment)
        {
            return 200;
        }
        else
        {
            return 405;
        }
    }

    public async Task<dynamic> NotifyBeforeExactDate()
    {
        DateTime tomorrow = DateTime.Today.AddDays(1);
        var getAllFromAppointment = await context.Set<TEntity>().Where(
            x => x.created_at.Date == tomorrow.Date
        ).ToListAsync();
        return getAllFromAppointment;
    }

    public async Task<dynamic> CheckAffectedSchedules(string start, string end)
    {
        int startIndex = start.IndexOf(" ", StringComparison.Ordinal) + 1;
        int endIndex = start.IndexOf(" GMT", StringComparison.Ordinal);
        string dateTimeString = start.Substring(startIndex, endIndex - startIndex);

        int startIndexForEnd = end.IndexOf(" ", StringComparison.Ordinal) + 1;
        int endIndexForEnd = end.IndexOf(" GMT", StringComparison.Ordinal);
        string dateTimeStringForEnd = end.Substring(startIndexForEnd, endIndexForEnd - startIndexForEnd);

        DateTimeOffset startdateTime = DateTimeOffset.Parse(dateTimeString);
        DateTimeOffset enddateTime = DateTimeOffset.Parse(dateTimeStringForEnd);
        var affectedSchedules = await context.Schedules.Where(
            x => x.start == startdateTime && x.isHoliday == 0 && x.status == 1
        ).ToListAsync();
        return affectedSchedules;
    }
    public async Task<dynamic> CheckHolidaysSchedules(string start, string end)
    {
        int startIndex = start.IndexOf(" ", StringComparison.Ordinal) + 1;
        int endIndex = start.IndexOf(" GMT", StringComparison.Ordinal);
        string dateTimeString = start.Substring(startIndex, endIndex - startIndex);

        DateTimeOffset startdateTime = DateTimeOffset.Parse(dateTimeString);
        var affectedSchedules = await context.Schedules.Where(
            x => x.start.Date == startdateTime.Date && x.isHoliday == 1 && x.status == 1
        ).ToListAsync();
        return affectedSchedules;
    }

    public async Task<dynamic> CheckSavedEventOnDB(int id)
    {
        var checkEvent = await context.Schedules.AnyAsync(x => x.id == id);
        if (checkEvent)
        {
            return 200;
        }
        else
        {
            return 201;
        }
    }

    public async Task<dynamic> CancelAppointmentLobby(int id)
    {
        var lobbyEntity = await context.Lobbies.Where(x => x.id == id).FirstOrDefaultAsync();
        if (lobbyEntity != null)
        {
            context.Lobbies.Remove(lobbyEntity);
            await context.SaveChangesAsync();
            return 200;
        }

        return 200;
    }

    public async Task<dynamic> PostNewHoliday(Schedule schedule)
    {
        schedule.start = Convert.ToDateTime(schedule.start).ToLocalTime();
        schedule.end = Convert.ToDateTime(schedule.end).ToLocalTime();
        await context.Schedules.AddAsync(schedule);
        await context.SaveChangesAsync();
        return 200;
    }

    public async Task<dynamic> checkStartDateIfHoliday(int id)
    {
        var checkIfHoliday = await context.Schedules.Where(x => x.id == id).FirstOrDefaultAsync();
        if (checkIfHoliday == null)
        {
            return 201;
        }
        else
        {
            if (checkIfHoliday.isHoliday == 1)
            {
                return 200;
            }
            else if (checkIfHoliday.isHoliday == 2)
            {
                return 202;
            }
            else
            {
                return 201;
            }
        }
    }

    public async Task<dynamic> removeAffectedSchedules(int id, int userid)
    {
        var findAllSchedulesById = await context.Schedules.Where(x => x.id == id).FirstOrDefaultAsync();
        var getUserByUserId = await context.UsersEnumerable.Where(x => x.id == userid).FirstOrDefaultAsync();
        if (getUserByUserId.access_level == 3)
        {
            // send email to remind customer of deletion of his/her schedule
            SendAppointmentEmailSMTPTWithoutCode(getUserByUserId.email,
                "Your schedule for" + " " + findAllSchedulesById.start.ToString("MMMM dd, yyyy dddd") + " " +
                "has been removed due to administrator holiday or closed drop schedule.");
            if (findAllSchedulesById != null)
            {
                context.Schedules.Remove(findAllSchedulesById);
                await context.SaveChangesAsync();
                return 200;
            }
        }
        else
        {
            if (findAllSchedulesById != null)
            {
                context.Schedules.Remove(findAllSchedulesById);
                await context.SaveChangesAsync();
                return 200;
            }

            return 400;
        }

        return 400;
    }

    public async Task<dynamic> getAllAppointmentPerBranch(int branch_id)
    {
        var findAllAppointmentsBasedOnBranch = await context.Set<TEntity>()
            .Where(x => x.branch_id == branch_id && x.status == 1 && x.isWalkedIn == 0).ToListAsync();
        return findAllAppointmentsBasedOnBranch;
    }

    public async Task<dynamic> getAllWalkedInPerBranch(int branch_id)
    {
        var findAllAppointmentsBasedOnBranch = await context.Set<TEntity>()
            .Where(x => x.branch_id == branch_id && x.status == 1 && x.isWalkedIn == 1).ToListAsync();
        return findAllAppointmentsBasedOnBranch;
    }

    public async Task<dynamic> getTodaysAppointment(int branch_id)
    {
        var getTodaysAppointment = await context.Set<TEntity>().Where(x => x.branch_id == branch_id
                                                                           && x.created_at.Date == DateTime.Today &&
                                                                           x.isWalkedIn == 0)
            .ToListAsync();
        return getTodaysAppointment;
    }

    public async Task<dynamic> CountSessionDone(int branch_id, int id)
    {
        int[] sessionStatuses = new[] { 1, 2, 0 };
        var result = await context.FollowUpAppointments.CountAsync(x =>
            sessionStatuses.Contains(x.isSessionStarted ?? 0) && x.branch_id == branch_id && x.id == id);
        return result;
    }

    public async Task<int> countAppointments(int branch_id, string type)
    {
        switch (type)
        {
            case "appointments_count":
                var result = await context.Set<TEntity>().CountAsync(x => x.isWalkedIn == 0);
                return result;
            case "todays_appointments":
                var todayCount = await context.Set<TEntity>().CountAsync(x => x.created_at.Date == DateTime.Today);
                return todayCount;
            case "done_appointments":
                var doneCounts = await context.Set<TEntity>().CountAsync(x => x.status == 2);
                return doneCounts;
            case "walkin_appointments":
                var walkin = await context.Set<TEntity>().CountAsync(x => x.isWalkedIn == 1);
                return walkin;
        }

        return 200;
    }

    public async Task<dynamic> FindPrimaryAppointments(int id)
    {
        var gained = await context.Appointments.Where(x => x.id == id).ToListAsync();
        return gained;
    }

    public async Task<int> CountAdminDashboardCountable(string type)
    {
        int[] managers = new[] { 1, 2, 3, 4, 5 };
        switch (type)
        {
            case "managers":
                var managersCount = await context.UsersEnumerable.CountAsync(x => managers.Contains(x.branch));
                return managersCount;
            case "customers":
                var customersCount = await context.UsersEnumerable.CountAsync(x => x.branch == 0);
                return customersCount;
            case "branches":
                var branchCount = await context.Branches.CountAsync();
                return branchCount;
            case "appointments":
                var doneAppointmentCount = await context.Set<TEntity>().CountAsync(x => x.status == 2);
                return doneAppointmentCount;
        }

        return 200;
    }
    public async Task<int> countAppointmentsCardCustomer(string type, string email)
    {
        switch (type)
        {
            case "appointments":
                var findAppointments = await context.Set<TEntity>().CountAsync(x => x.email == email);
                return findAppointments;
            case "inprogress-appointments":
                var inProgressAppointments = await context.Set<TEntity>()
                    .CountAsync(x => x.isSessionStarted == 1 && x.email == email);
                return inProgressAppointments;
            case "done-appointments":
                var doneAppointments = await context.Set<TEntity>()
                    .CountAsync(x => x.email == email && x.status == 2);
                return doneAppointments;
        }

        return 200;
    }
    public async Task<dynamic> FindAppointmentsByEmail(string email)
    {
        int[] stats = new int[] { 1, 2 };
        var appointmentByEmail = await context.Set<TEntity>().Where(x => x.email == email && stats.Contains(x.status)).ToListAsync();
        return appointmentByEmail;
    }



    public async Task<dynamic> FindRecordManagementPerBranch(int branch_id)
    {
        var result = await context.Set<TEntity>().Where(x => x.status == 2 && x.branch_id == branch_id).ToListAsync();
        return result;
    }

    public async Task<dynamic> GetAllAppointmentBranch()
    {
        int[] allBranches = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var result = await context.Set<TEntity>()
            .Where(x => allBranches.Contains(x.branch_id) && x.status == 2).ToListAsync();
        return result;
    }

    public async Task<dynamic> findUserByManagerId(int manager_id)
    {
        var result = await context.UsersEnumerable.Where(x => x.id == manager_id).FirstOrDefaultAsync();
        return result;
    }

    public async Task<dynamic> FindFollowUpsOnRecordManagement(int id)
    {
        var gained = await context.FollowUpAppointments.Where(x => x.id == id).ToListAsync();
        return gained;
    }


    public async Task<dynamic> createFollowUpAppointment(FollowUpAppointment followUpAppointment)
    {
        var smsProvider = new SMSTwilioService();
        var updateEntity = await context.Set<TEntity>().Where(x => x.id == followUpAppointment.id)
            .FirstOrDefaultAsync();
        if (followUpAppointment.start <= DateTime.Today || followUpAppointment.end <= DateTime.Today)
        {
            return 401;
        }
        else
        {
            followUpAppointment.start = followUpAppointment.start.AddDays(1);
            followUpAppointment.end = followUpAppointment.end.AddDays(1);
            await context.FollowUpAppointments.AddAsync(followUpAppointment);
            await context.SaveChangesAsync();
            if (followUpAppointment.notificationType == "email")
            {
                SendAppointmentEmailSMTPTWithoutCode(updateEntity.email, "You have a follow-up appointment on " + " " + followUpAppointment.start.ToString("MMMM dd, yyyy dddd") + "." + "Kindly please attend.");
            }
            else
            {
                var getApiKey = await context.TwillioAuths
                    .Where(x => x.identifier == "twilio-auth").FirstOrDefaultAsync();
                smsProvider.SendSMSService(
                    "You have a follow-up appointment on " + " " + followUpAppointment.start.ToString("MMMM dd, yyyy dddd") + "." + "Kindly please attend.",
                    "+63" + updateEntity.phoneNumber, getApiKey.accountSID, getApiKey.authtoken
                );
            }
            return 200;
        }
    }

    public async Task<dynamic> checkIfAppointmentIsDone()
    {
        var result = await context.Set<TEntity>().AnyAsync(x => x.status == 0);
        if (result)
        {
            return true;
        }

        return false;
    }

    public async Task<dynamic> AppointmentSession(SessionActions sessionActions)
    {
        var startSession = await context.Set<TEntity>().Where(x => x.id == sessionActions.id).FirstOrDefaultAsync();
        var deleteEntity = await context.Set<TEntity>().Where(x => x.id == sessionActions.id)
            .FirstOrDefaultAsync();
        switch (sessionActions.actions)
        {
            case "start":
                startSession.isSessionStarted = 1;
                startSession.managersId = sessionActions.managerUid;
                await context.SaveChangesAsync();
                return 201;
            case "end":
                startSession.isSessionStarted = 2;
                await context.SaveChangesAsync();
                return 202;
            case "cancel_appointment":
                ScheduleEntity[] entites = JsonSerializer.Deserialize<ScheduleEntity[]>(deleteEntity.appointmentSchedule);
                if (deleteEntity != null)
                {
                    foreach (ScheduleEntity entity in entites)
                    {
                        var scheduleDeletion =
                            await context.Schedules.Where(x => x.id == sessionActions.deletionId).FirstOrDefaultAsync();
                        if (scheduleDeletion != null)
                        {
                            context.Schedules.Remove(scheduleDeletion);
                            context.Set<TEntity>().Remove(deleteEntity);
                            await context.SaveChangesAsync();
                            return 200;
                        }
                    }
                }

                return 203;
        }

        return 200;
    }

    public async Task<dynamic> GetAssignedSessionUsers(int manageruid)
    {
        var result = await context.UsersEnumerable.Where(x => x.id == manageruid)
            .FirstOrDefaultAsync();
        return result;
    }




    public async Task<dynamic> FollowUpAppointmentsList(int branch_id, int appointmentId)
    {
        var findAllFollowUpAppointmentsBasedOnBranch = await context.FollowUpAppointments
            .Where(x => x.branch_id == branch_id && x.id == appointmentId).ToListAsync();
        return findAllFollowUpAppointmentsBasedOnBranch;
    }

    public IQueryable<dynamic> SearchFollowUpAppointments(string start, string end, string customerName)
    {
        var searchQuery = context.FollowUpAppointments.AsQueryable();
        if (string.IsNullOrWhiteSpace(start) || string.IsNullOrWhiteSpace(end) && !string.IsNullOrWhiteSpace(customerName))
        {
            searchQuery = searchQuery.Where(o => o.customerName.Contains(customerName));
            return searchQuery;
        }
        else if (string.IsNullOrEmpty(customerName) || customerName.Contains("null"))
        {
            int startIndex = start.IndexOf(" ", StringComparison.Ordinal) + 1;
            int endIndex = start.IndexOf(" GMT", StringComparison.Ordinal);
            string dateTimeString = start.Substring(startIndex, endIndex - startIndex);

            int startIndexForEnd = end.IndexOf(" ", StringComparison.Ordinal) + 1;
            int endIndexForEnd = end.IndexOf(" GMT", StringComparison.Ordinal);
            string dateTimeStringForEnd = end.Substring(startIndexForEnd, endIndexForEnd - startIndexForEnd);

            DateTimeOffset startdateTime = DateTimeOffset.Parse(dateTimeString);
            DateTimeOffset enddateTime = DateTimeOffset.Parse(dateTimeStringForEnd);

            searchQuery = searchQuery.Where(o => o.start >= startdateTime.Date && o.start <= enddateTime.Date.AddDays(1));
            return searchQuery;
        }
        else
        {
            int startIndex = start.IndexOf(" ", StringComparison.Ordinal) + 1;
            int endIndex = start.IndexOf(" GMT", StringComparison.Ordinal);
            string dateTimeString = start.Substring(startIndex, endIndex - startIndex);

            int startIndexForEnd = end.IndexOf(" ", StringComparison.Ordinal) + 1;
            int endIndexForEnd = end.IndexOf(" GMT", StringComparison.Ordinal);
            string dateTimeStringForEnd = end.Substring(startIndexForEnd, endIndexForEnd - startIndexForEnd);

            DateTimeOffset startdateTime = DateTimeOffset.Parse(dateTimeString);
            DateTimeOffset enddateTime = DateTimeOffset.Parse(dateTimeStringForEnd);


            searchQuery = searchQuery.Where(o => o.customerName.Contains(customerName) && o.start.Date >= startdateTime && o.start <= enddateTime.Date.AddDays(1));
            return searchQuery;
        }
    }

    public async Task<dynamic> FollowUpAppointmentSession(FollowUpSessionActions followUpSessionActions)
    {
        var sessionManagement = await context.FollowUpAppointments.Where(x => x.followupId == followUpSessionActions.id)
            .FirstOrDefaultAsync();
        var deleteEntity = await context.Set<TEntity>().Where(x => x.id == followUpSessionActions.id)
            .FirstOrDefaultAsync();
        switch (followUpSessionActions.actions)
        {
            case "start":
                sessionManagement.isSessionStarted = 1;
                await context.SaveChangesAsync();
                return 200;
            case "end":
                sessionManagement.isSessionStarted = 2;
                await context.SaveChangesAsync();
                return 201;
            case "done":
                sessionManagement.status = 2;
                sessionManagement.isSessionStarted = 3;
                await context.SaveChangesAsync();
                return 202;
            case "cancel_appointment":
                if (deleteEntity != null)
                {
                    context.Set<TEntity>().Remove(deleteEntity);
                    await context.SaveChangesAsync();
                    return 200;
                }

                return 202;
        }

        return 202;
    }

    public async Task<List<TEntity>> FilterRecordsByBranch(int branch_id)
    {
        var foundBranch = await context.Branches.Where(x => x.branchKey == "all")
            .FirstOrDefaultAsync();
        var filtered = await context.Set<TEntity>().Where(
            x => (x.branch_id == branch_id && x.status == 2) || x.branch_id == foundBranch.branch_id).ToListAsync();
        return filtered;
    }

    public async Task<dynamic> UpdateStatusToArchiveAppointment(int id)
    {
        var updateToArchive = await context.Set<TEntity>()
        .Where(x => x.id == id).FirstOrDefaultAsync();
        if (updateToArchive != null)
        {
            updateToArchive.status = 3;
            await context.SaveChangesAsync();
            return 200;
        }
        return 400;
    }

    public async Task<dynamic> DeleteRecords(int id)
    {
        var result = await context.Appointments.Where(x => x.id == id)
        .FirstOrDefaultAsync();
        if (result != null)
        {
            context.Appointments.Remove(result);
            await context.SaveChangesAsync();
            return 200;
        }
        return 400;
    }
}