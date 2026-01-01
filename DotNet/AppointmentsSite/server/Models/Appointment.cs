using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace server.Models
{
  public class Doctor
  {
    [Key]
    public long Id { get; set; }

    public string Name { get; set; }
  }

  public class FreeTime
  {
    [Key]
    public long Id { get; set; }

    public Doctor Doctor { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }
  }

  public class Information
  {
    [Key]
    public long Id { get; set; }

    public string PatientName { get; set; }
    public string PatientPhone { get; set; }
    public string AdditionalInfo { get; set; }
  }



  public class Appointment
  {
    [Key]
    public long Id { get; set; }


    public Doctor Doctor { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public Information Info { get; set; }

    public string Status { get; set; }
    public DateTime Created { get; set; }


    public string EventId { get; set; }


    [Timestamp]
    public byte[] RowVersion { get; set; }
  }
}
