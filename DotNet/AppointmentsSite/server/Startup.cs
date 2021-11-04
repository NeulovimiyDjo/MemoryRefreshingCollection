using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using server.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace server
{
  public class Startup
  {
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddHttpsRedirection(options => {
        options.HttpsPort = 443;
      });


      services.AddSingleton<Services.TelegramBotService>();

      services.AddDbContext<AppointmentsContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

      services.AddSingleton<Services.Calendar>();


      services.AddDbContext<IdentityContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

      services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<IdentityContext>();
      // should be added after the  AddIdentity
      services.ConfigureApplicationCookie(identityOptionsCookies =>
      {
        identityOptionsCookies.Cookie.Name = "AuthCookie";
        identityOptionsCookies.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        identityOptionsCookies.Cookie.SameSite = SameSiteMode.None;
        identityOptionsCookies.Events.OnRedirectToLogin = context =>
        {
          context.Response.StatusCode = StatusCodes.Status401Unauthorized;
          return Task.CompletedTask;
        };
      });


      services.AddAntiforgery(options =>
      {
        options.Cookie.Name = "AntiforgeryCookie";
        options.HeaderName = "X-CSRF-TOKEN";
      });


      services.AddTransient<Filters.AntiforgeryCookieResultFilterAttribute>();

      services.AddMvc(options =>
      {
        options.Filters.AddService<Filters.AntiforgeryCookieResultFilterAttribute>();
      });


      services.AddCors();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, IAntiforgery antiforgery)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseCors(builder => builder.WithOrigins("http://localhost:8080").AllowAnyHeader().AllowAnyMethod().AllowCredentials());
      }
      else
      {
        //app.UseHsts();
      }

      //app.UseHttpsRedirection();


      app.Use(async (context, next) =>
      {
        var path = context.Request.Path.Value;

        if (!path.StartsWith("/api") && !System.IO.Path.HasExtension(path))
        {
          context.Request.Path = "/index.html";
          await next();
        }
        else
        {
          await next();
        }
      });

      app.UseStaticFiles();


      app.UseAuthentication();

      app.UseMvc();
    }
  }
}
