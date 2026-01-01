using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace server.Models
{
  public class AppointmentsContext : DbContext
  {
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<FreeTime> FreeTimes { get; set; }
    public DbSet<Appointment> Appointments { get; set; }


    public AppointmentsContext(DbContextOptions<AppointmentsContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Appointment>()
       .HasOne(a => a.Info)
       .WithOne()
       .HasForeignKey("Information")
       .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
