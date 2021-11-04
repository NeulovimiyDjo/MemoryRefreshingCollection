using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace server.Controllers
{
  [Route("api/[controller]/[action]")]
  [ApiController]
  public class AccountController : ControllerBase
  {
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
      _userManager = userManager;
      _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult IsLoggedIn()
    {
      return new JsonResult(User.Identity.IsAuthenticated);
    }

    public struct RegisterInfo
    {
      public string email;
      public string userName;
      public string password;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<string>> Register([FromBody]RegisterInfo info)
    {
      Response.ContentType = "application/json";

      if (ModelState.IsValid)
      {
        IdentityUser user = new IdentityUser { Email = info.email, UserName = info.userName };

        var result = await _userManager.CreateAsync(user, info.password);
        if (result.Succeeded)
        {
          await _signInManager.SignInAsync(user, false);

          HttpContext.User = await _signInManager.CreateUserPrincipalAsync(user);
        }
        else
        {
          string errors = "";
          foreach (var error in result.Errors)
          {
            errors += error.Description;
          }
          return Ok(errors);
        }
      }

      return Ok("Registered");
    }

    public struct LoginInfo
    {
      public string userName;
      public string password;
      public bool remember;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<string>> Login([FromBody]LoginInfo info)
    {
      Response.ContentType = "application/json";

      if (ModelState.IsValid)
      {
        var result = await _signInManager.PasswordSignInAsync(info.userName, info.password, info.remember, false);
        if (result.Succeeded)
        {
          var user = await _userManager.FindByNameAsync(info.userName);
          HttpContext.User = await _signInManager.CreateUserPrincipalAsync(user);

          return Ok("Success");
        }
        else
        {
          return Ok("Fail");
        }
      }

      return Ok("ModelState not valid");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<string>> Logout()
    {
      Response.ContentType = "application/json";

      await _signInManager.SignOutAsync();

      HttpContext.User = new ClaimsPrincipal();

      return Ok("Success");
    }
  }
}
