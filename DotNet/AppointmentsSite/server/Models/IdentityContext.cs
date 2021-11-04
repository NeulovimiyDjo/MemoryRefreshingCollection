using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace server.Models
{
  public class IdentityContext : IdentityDbContext<IdentityUser>
  {
    public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
    {
    }
  }
}
