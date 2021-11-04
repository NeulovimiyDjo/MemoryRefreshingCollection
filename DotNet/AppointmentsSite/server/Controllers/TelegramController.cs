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
  [Route("api/[controller]")]
  public class TelegramController : ControllerBase
  {
    private readonly Services.TelegramBotService _botService;

    public TelegramController(Services.TelegramBotService botService)
    {
      _botService = botService;
    }

    // POST api/update
    [HttpPost]
    public async Task Post([FromBody]Update update)
    {
      if (update == null) return;
      var message = update.Message;
      if (message?.Type == MessageType.Text)
      {
        await _botService.Client.SendTextMessageAsync(message.Chat.Id, message.Text + "  Test Echo");
      }
    }
  }
}
