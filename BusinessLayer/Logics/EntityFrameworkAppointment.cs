using System.Globalization;
using System.Linq.Expressions;
using abys_agrivet_backend.DB;
using abys_agrivet_backend.Helper.MailSettings;
using abys_agrivet_backend.Helper.Schedule;
using abys_agrivet_backend.Helper.SearchEngine;
using abys_agrivet_backend.Helper.SessionActions;
using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Model;
using abys_agrivet_backend.Repository.Appointment;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;

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
    public async Task<TEntity> makeAnAppointment(TEntity entity)
    {
        if (entity.reminderType == 1)
        {
            /*email service from smtp*/
            SendAppointmentEmailSMTPTWithoutCode(entity.email, "Thank you for making an appointment we will notify you ahead of time");
            await context.Set<TEntity>().AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        else
        {
            return entity;
        }
    }

    public async Task<dynamic> createSchedule(Schedule schedule)
    {
        var checkDay = await context.Schedules.AnyAsync(x => x.start == schedule.start);
        var identifier = await context.Schedules.AnyAsync(x => x.start == schedule.start);
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

    public async Task<dynamic> GetAllSchedulePerBranch(int branch)
    {
        var getAllSchedule = await context.Schedules.Where(x => x.branch == branch || x.isHoliday == 1 || x.status == 0)
            .Select(t => new
            {
                t.id,
                t.title,
                t.start,
                t.end,
                isHoliday = t.isHoliday == 1 ? true : false
            }).ToListAsync();
        return getAllSchedule;
    }

    public async Task<dynamic> RemoveSelectedSchedule(int id)
    {
        var entityRemove = await context.Schedules.FindAsync(id);
        if (entityRemove != null)
        {
            context.Schedules.Remove(entityRemove);
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
        if (checkFromSchedules)
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
        DateTime currentDate = DateTime.Now.Date;
        DateTime currentDateMinusOneDay = currentDate.AddDays(1);
        var getAllFromAppointment = await context.Schedules.Where(
            x => x.start == currentDateMinusOneDay
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
            x => x.start >= startdateTime && x.start <= enddateTime.AddDays(1)
        ).ToListAsync();
        return affectedSchedules;
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
            } else if (checkIfHoliday.status == 0)
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
            SendAppointmentEmailSMTPTWithoutCode(getUserByUserId.email, "Your schedule for" + " " + findAllSchedulesById.start.ToString("MMMM dd, yyyy dddd") + " " + "has been removed due to administrator holiday or closed drop schedule.");
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
            .Where(x => x.branch_id == branch_id && x.status == 1 ).ToListAsync();
        return findAllAppointmentsBasedOnBranch;
    }

  
    public async Task<dynamic> getTodaysAppointment(int branch_id)
    {
        var getTodaysAppointment = await context.Set<TEntity>().Where(x => x.branch_id == branch_id
                                                                           && x.created_at.Date == DateTime.Today)
            .ToListAsync();
        return getTodaysAppointment;
    }

    public async Task<dynamic> CountSessionDone(int branch_id, int id)
    {
        var result = await context.FollowUpAppointments.Where(x =>  x.branch_id == branch_id).FirstOrDefaultAsync();
        return result.isSessionStarted;
    }

    public async Task<dynamic> createFollowUpAppointment(FollowUpAppointment followUpAppointment)
    {
        var updateEntity = await context.Set<TEntity>().Where(x => x.id == followUpAppointment.id)
            .FirstOrDefaultAsync();
        if (followUpAppointment.start <= DateTime.Today || followUpAppointment.end <= DateTime.Today)
        {
            return 401;
        }
        else
        {
            await context.FollowUpAppointments.AddAsync(followUpAppointment);
            await context.SaveChangesAsync();
            if (followUpAppointment.notificationType == "email")
            {
                SendAppointmentEmailSMTPTWithoutCode(updateEntity.email, "You have a follow-up appointment on " + " " + followUpAppointment.start.ToString("MMMM dd, yyyy dddd") + "." + "Kindly please attend.");
            } // add else condition if there is an sms service..
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
        }

        return 200;
    }
    
    public async Task<dynamic> GetAssignedSessionUsers(int manageruid)
    {
        var result = await context.UsersEnumerable.Where(x => x.id == manageruid)
            .FirstOrDefaultAsync();
        return result;
    }

    public async Task<dynamic> AppointmentMakeItDone(int id)
    {
        var updateEntity = await context.Set<TEntity>().Where(x => x.id == id)
            .FirstOrDefaultAsync();
        if (updateEntity != null)
        {
            updateEntity.status = 2;
            await context.SaveChangesAsync();
            return 200;
        }

        return 201;
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
        } else if (string.IsNullOrEmpty(customerName) || customerName.Contains("null"))
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
        }

        return 202;
    }

}