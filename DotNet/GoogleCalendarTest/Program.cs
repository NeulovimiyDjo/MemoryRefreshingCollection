using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CalendarQuickstart
{
  class Program
  {
    static string[] Scopes = { CalendarService.Scope.Calendar };
    static string ApplicationName = "SDSG";

    static void Main(string[] args)
    {
      GoogleCredential credential;

      using (var stream =
          new FileStream("Data/server_server_cred.json", FileMode.Open, FileAccess.Read))
      {
 
        credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
      }

      // Create Google Calendar API service.
      var service = new CalendarService(new BaseClientService.Initializer()
      {
        HttpClientInitializer = credential,
        ApplicationName = ApplicationName,
      });

      string calendarId = File.ReadAllText("Data/calendar_id.txt");
      // Define parameters of request.
      EventsResource.ListRequest request = service.Events.List(calendarId);
      request.TimeMin = DateTime.Now;
      request.ShowDeleted = false;
      request.SingleEvents = true;
      request.MaxResults = 10;
      request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

      // List events.
      Events events = request.Execute();
      Console.WriteLine("Upcoming events:");
      if (events.Items != null && events.Items.Count > 0)
      {
        foreach (var eventItem in events.Items)
        {
          string start = eventItem.Start.DateTime.ToString();
          string end = eventItem.End.DateTime.ToString();
          Console.WriteLine($"{eventItem.Summary} ({start}) - ({end})");
        }
      }
      else
      {
        Console.WriteLine("No upcoming events found.");
      }


      // add event
      Event newEvent = new Event()
      {
        Summary = "test_added_event1",
        Location = "800 Howard St., San Francisco, CA 94103",
        Description = "A chance to hear more about Google's developer products.",
        Start = new EventDateTime()
        {
          DateTime = DateTime.Parse("2019-04-25T09:00:00-07:00"),
          TimeZone = "America/Los_Angeles",
        },
        End = new EventDateTime()
        {
          DateTime = DateTime.Parse("2019-04-25T17:00:00-07:00"),
          TimeZone = "America/Los_Angeles",
        },
        //Recurrence = new String[] { "RRULE:FREQ=DAILY;COUNT=2" },
        Attendees = new EventAttendee[] {
        new EventAttendee() { Email = "lpage@example.com" },
        new EventAttendee() { Email = "sbrin@example.com" },
    },
        Reminders = new Event.RemindersData()
        {
          UseDefault = false,
          Overrides = new EventReminder[] {
            new EventReminder() { Method = "email", Minutes = 24 * 60 },
            new EventReminder() { Method = "sms", Minutes = 10 },
        }
        }
      };

      EventsResource.InsertRequest request2 = service.Events.Insert(newEvent, calendarId);
      Event createdEvent = request2.Execute();
      Console.WriteLine("Event created: {0}", createdEvent.HtmlLink);


      Console.Read();

    }
  }
}