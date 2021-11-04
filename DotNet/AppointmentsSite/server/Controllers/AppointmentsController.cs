using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Globalization;
using server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace server.Controllers
{
  [Route("api/[controller]/[action]")]
  [ApiController]
  public class AppointmentsController : ControllerBase
  {
    private readonly Services.Calendar _calendar;
    private readonly AppointmentsContext _dbContext;
    private readonly Services.TelegramBotService _botService;


    public AppointmentsController(Services.Calendar calendar, AppointmentsContext context, Services.TelegramBotService botService)
    {
      _calendar = calendar;

      _dbContext = context;

      _botService = botService;
    }


    private string StringDate(DateTime dt)
    {
      return dt.Year + "-" + dt.Month.ToString("00") + "-" + dt.Day.ToString("00");
    }

    private string StringTime(DateTime dt)
    {
      return dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00");
    }


    private List<dynamic> CalculateIntersectingElements(dynamic newEl, IEnumerable<dynamic> list)
    {
      var intersectingElements = new List<dynamic>();

      foreach (var el in list)
      {
        if (el.Id != newEl.Id && !(el.End <= newEl.Start || el.Start >= newEl.End)) // el and newEl intersect in time
        {
          intersectingElements.Add(el);
        }
      }

      intersectingElements.Sort((e1, e2) => e1.Start.CompareTo(e2.Start));

      return intersectingElements;
    }



    [Authorize]
    [HttpGet]
    public IActionResult GetDb()
    {
      var appointments = _dbContext.Appointments.AsNoTracking().Include(a => a.Doctor).Include(a => a.Info).Select(a => new {
        Id = a.Id,
        RowVersion = a.RowVersion,
        Status = a.Status,
        Doctor = a.Doctor.Name,
        Patient = a.Info.PatientName,
        Phone = a.Info.PatientPhone,
        Info = a.Info.AdditionalInfo,
        Date = StringDate(a.Start),
        Start = StringTime(a.Start),
        End = StringTime(a.End),
        Duration = (a.End - a.Start).TotalMinutes,
        Created = StringDate(a.Created) + " " + StringTime(a.Created)
      });

      var freeTimes = _dbContext.FreeTimes.AsNoTracking().Include(a => a.Doctor).Select(ft => new {
        Id = ft.Id,
        RowVersion = ft.RowVersion,
        Doctor = ft.Doctor.Name,
        Date = StringDate(ft.Start),
        Start = StringTime(ft.Start),
        End = StringTime(ft.End)
      });

      var doctors = _dbContext.Doctors.Select(p => new { p.Name }).ToList();

      return Ok(new { Appointments = appointments, FreeTimes = freeTimes, Doctors = doctors });
    }



    public struct AInfo
    {
      public long id;
      public byte[] rowVersion;
      public string date;
      public string start;
      public int duration;
      public string info;
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmAppointment([FromBody]AInfo info)
    {
      byte[] newRowVersion = null;
      try
      {
        var appointment = await _dbContext.Appointments.AsNoTracking().Include(a => a.Doctor).Include(a => a.Info).SingleOrDefaultAsync(a => a.Id == info.id);

        if (appointment == null)
          return Ok("Fail");

        bool update = false;
        if (appointment.Status == "Confirmed")
          update = true;


        appointment.RowVersion = info.rowVersion;
        appointment.Status = "Confirmed";

        var start = DateTime.ParseExact(info.date + " " + info.start, "yyyy-MM-dd HH:mm", null);
        appointment.Start = start;
        appointment.End = start.AddMinutes(info.duration);
        appointment.Info.AdditionalInfo = info.info;



        var appointments = _dbContext.Appointments.AsNoTracking().Include(ft => ft.Doctor)
          .Where(a => a.Status == "Confirmed" && a.Doctor.Name == appointment.Doctor.Name && a.Start.Date == appointment.Start.Date);

        var intersectingElements = CalculateIntersectingElements(appointment, appointments);
        if (intersectingElements.Count > 0)
        {
          return Ok("Intersection in time");
        }

        if (!update)
        {
          appointment.EventId = Guid.NewGuid().ToString("N");
        }

        _dbContext.Appointments.Update(appointment);
        _dbContext.SaveChanges();

        newRowVersion = appointment.RowVersion;


        Event newEvent = new Event()
        {
          Id = appointment.EventId,
          Summary = appointment.Info.PatientName + " - " + appointment.Doctor.Name,
          Description = "Procedure: " + appointment.Info.AdditionalInfo +
            "\nDoctor Name: " + appointment.Doctor.Name + "\nPatient Name: " + appointment.Info.PatientName,
          //Attendees = new EventAttendee[]
          //{
          //  new EventAttendee{ DisplayName = info.patient, Email = "" },
          //  new EventAttendee{ DisplayName = info.doctor, Email = "" },
          //},
          Start = new EventDateTime()
          {
            DateTime = appointment.Start,
          },
          End = new EventDateTime()
          {
            DateTime = appointment.End,
          }
        };

        if (update)
        {
          var eventId = appointment.EventId;
          var request = _calendar.Service.Events.Update(newEvent, _calendar.CalendarId, eventId);
          Event createdEvent = request.Execute();
        }
        else
        {
          var request = _calendar.Service.Events.Insert(newEvent, _calendar.CalendarId);
          Event createdEvent = await request.ExecuteAsync();
        }
      }
      catch (DbUpdateConcurrencyException)
      {
        return Ok("Fail");
      }

      return Ok(new { status = "Confirmed", newRowVersion = newRowVersion });
    }


    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAppointment([FromBody]AInfo info)
    {
      try
      {
        var appointment = await _dbContext.Appointments.AsNoTracking().Include(a => a.Doctor).Include(a => a.Info).SingleOrDefaultAsync(a => a.Id == info.id);

        if (appointment == null)
          return Ok("Fail");

        appointment.RowVersion = info.rowVersion;

        _dbContext.Appointments.Remove(appointment);
        _dbContext.SaveChanges();

        if (appointment.Status == "Confirmed")
        {
          var eventId = appointment.EventId;
          var request = _calendar.Service.Events.Delete(_calendar.CalendarId, eventId);
          await request.ExecuteAsync();
        }
      }
      catch (DbUpdateConcurrencyException)
      {
        return Ok("Fail");
      }

      return Ok("Removed");
    }


    public struct AInfo2
    {
      public long id;
      public byte[] rowVersion;
      public string doctor;
      public string date;
      public string start;
      public string end;
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFreeTime([FromBody]AInfo2 info)
    {
      if (info.doctor == "" || info.date == "" || info.start == "" || info.end == "")
        return Ok("Invalid info");

      try
      {
        var freeTime = await _dbContext.FreeTimes.AsNoTracking().Include(a => a.Doctor).SingleOrDefaultAsync(ft => ft.Id == info.id);

        bool adding = false;
        if (freeTime == null)
        {
          freeTime = new FreeTime {
            Doctor = _dbContext.Doctors.SingleOrDefault(d => d.Name == info.doctor)
          };

          adding = true;
        }
        else
        {
          freeTime.RowVersion = info.rowVersion;
        }

        freeTime.Start = DateTime.ParseExact(info.date + " " + info.start, "yyyy-MM-dd HH:mm", null);
        freeTime.End = DateTime.ParseExact(info.date + " " + info.end, "yyyy-MM-dd HH:mm", null);



        var freeTimes = _dbContext.FreeTimes.AsNoTracking().Include(ft => ft.Doctor)
          .Where(ft => ft.Doctor.Name == freeTime.Doctor.Name && ft.Start.Date == freeTime.Start.Date);


        var intersectingElements = CalculateIntersectingElements(freeTime, freeTimes);
        if (intersectingElements.Count > 0)
        {
          var newFt = new FreeTime{ Doctor = freeTime.Doctor };
          newFt.Start = intersectingElements.First().Start < freeTime.Start ? intersectingElements.First().Start : freeTime.Start;
          newFt.End = intersectingElements.Last().End > freeTime.End ? intersectingElements.Last().End : freeTime.End;

          if (!adding)
          {
            _dbContext.FreeTimes.Remove(freeTime);
          }
          _dbContext.RemoveRange(intersectingElements.Select(el => new FreeTime { Id = el.Id, RowVersion = el.RowVersion }));


          _dbContext.FreeTimes.Add(newFt);
          _dbContext.SaveChanges();

          return Ok("Success");

          //return Ok("Intersection error");
        }



        if (adding)
        {
          _dbContext.FreeTimes.Add(freeTime);
        }
        else
        {
          _dbContext.FreeTimes.Update(freeTime);
        }

        _dbContext.SaveChanges();
      }
      catch (DbUpdateConcurrencyException)
      {
        return Ok("Fail");
      }

      return Ok("Success");
    }


    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFreeTime([FromBody]AInfo info)
    {
      try
      {
        var freeTime = await _dbContext.FreeTimes.AsNoTracking().Include(a => a.Doctor).SingleOrDefaultAsync(ft => ft.Id == info.id);

        if (freeTime == null)
          return Ok("Fail");



        //var appointments = _dbContext.Appointments.AsNoTracking().Include(a => a.Doctor)
        //  .Where(a => a.Status == "Confirmed" && a.Doctor.Name == freeTime.Doctor.Name && a.Start.Date == freeTime.Start.Date);

        //var intersectedAppointments = new List<Appointment>();
        //foreach (var a in appointments)
        //{
        //  if (!(a.End <= freeTime.Start || a.Start >= freeTime.End)) // a and freeTime intersect in time
        //  {
        //    intersectedAppointments.Add(a);
        //  }
        //}


        freeTime.RowVersion = info.rowVersion;

        _dbContext.FreeTimes.Remove(freeTime);
        _dbContext.SaveChanges();
      }
      catch (DbUpdateConcurrencyException)
      {
        return Ok("Fail");
      }

      return Ok("Removed");
    }



    private (bool, DateTime) IsAvailable(DateTime dt, List<Appointment> appointments, long doctorId)
    {
      var start = dt;
      var end = dt.AddMinutes(30);

      foreach (var a in appointments)
      {
        if (doctorId == a.Doctor.Id && a.End > start.AddSeconds(1) && a.Start < end.AddSeconds(-1))
          return (false, a.End);
      }

      return (true, new DateTime());
    }

    [HttpGet]
    public IActionResult GetAvailableDateTimes()
    {
      var allDateTimes = new Dictionary<string, List<dynamic>>();

      var freeTimes = _dbContext.FreeTimes.AsNoTracking().Include(ft => ft.Doctor);
      var appointments = _dbContext.Appointments.AsNoTracking().Include(a => a.Doctor).Where(a => a.Status == "Confirmed").ToList();

      foreach (var ft in freeTimes)
      {
        List<dynamic> dateTimes;
        if (allDateTimes.ContainsKey(ft.Doctor.Name))
          dateTimes = allDateTimes[ft.Doctor.Name];
        else
          dateTimes = new List<dynamic>();

        int count = 0;
        var dt = ft.Start;
        while (dt.AddMinutes(30) < ft.End.AddSeconds(1))
        {
          var check = IsAvailable(dt, appointments, ft.Doctor.Id);
          if (!check.Item1)
          {
            //dt = dt.AddMinutes(30);

            dt = (check.Item2.Minute % 30 == 0) ? check.Item2 : check.Item2.AddMinutes(30 - check.Item2.Minute % 30);
            continue;
          }

          dateTimes.Add(new { Date = StringDate(dt), Time = StringTime(dt), EndTime = StringTime(dt.AddMinutes(30)) });
          dt = dt.AddMinutes(30);

          if (++count > 24 * 60 / 30) break;
        }

        allDateTimes[ft.Doctor.Name] = dateTimes;
      }

      return Ok(allDateTimes);
    }



    private string GetFreeTimesString(string doctorName, DateTime date)
    {
      var appointments = _dbContext.Appointments.AsNoTracking().Include(a => a.Doctor)
        .Where(a => a.Status == "Confirmed" && a.Doctor.Name == doctorName && a.Start.Date == date)
        .Select(a => new { Start = a.Start, End = a.End });

      var freeTimes = _dbContext.FreeTimes.AsNoTracking().Include(ft => ft.Doctor)
        .Where(ft => ft.Doctor.Name == doctorName && ft.Start.Date == date)
        .Select(ft => new { Start = ft.Start, End = ft.End });



      var difference = new[] { new { Start = new DateTime(), End = new DateTime() } }.ToList();
      difference.Clear();

      foreach (var ft in freeTimes)
      {
        var start = ft.Start;

        foreach (var a in appointments)
        {
          if (a.End <= ft.Start || a.Start >= ft.End) continue;

          if (start.AddMinutes(5) < a.Start)
          {
            difference.Add(new { Start = start, End = a.Start });
          }

          start = a.End;
        }

        if (start.AddMinutes(5) < ft.End)
        {
          difference.Add(new { Start = start, End = ft.End });
        }
      }

      difference.Sort((e1, e2) => e1.Start.CompareTo(e2.Start));



      string res = "";

      foreach (var range in difference)
      {
        if (res != "") res += ", ";

        res += StringTime(range.Start) + " - " + StringTime(range.End);
      }

      return res;
    }

    public struct AppointmentInfo
    {
      public string patient;
      public string phone;
      public string doctor;
      public string date;
      public string time;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAppointment([FromBody]AppointmentInfo info)
    {
      if (info.patient == "" || info.phone == "" || info.doctor == "" || info.date == "" || info.time == "")
        return Ok("Invalid info");



      var start = DateTime.ParseExact(info.date + " " + info.time, "yyyy-MM-dd HH:mm", null);

      var appointment = new Appointment {
        Doctor = _dbContext.Doctors.First(d => d.Name == info.doctor),
        Start = start,
        End = start.AddMinutes(30),
        Info = new Information { PatientName = info.patient, PatientPhone = info.phone },
        Status = "Unconfirmed",
        Created = DateTime.UtcNow.AddHours(3)
      };

      _dbContext.Appointments.Attach(appointment);
      _dbContext.SaveChanges();


      var freeTimesString = GetFreeTimesString(info.doctor, start.Date);

      await _botService.SendMessageToGroupAsync("Не подтвержденная запись\nФИО пациента: " + info.patient + "\nНомер телефона: " + info.phone
          + "\nВрач: " + info.doctor + "\nДата: " + info.date + "\nВремя: " + info.time
          + "\nСвободное время: " + freeTimesString + "\nАдминка линк: https://" + Request.Host + "/EditPage/" + appointment.Id);
      
      
     
      return Ok("Created");
    }
  }
}
