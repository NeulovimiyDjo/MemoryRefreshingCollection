using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.AspNetCore.Hosting;

namespace server.Services
{
  public class TelegramBotService
  {
    public TelegramBotClient Client { get; }

    private long _groupChatId;
    private bool _dev = false;


    public TelegramBotService(IHostingEnvironment env)
    {
      string appDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName;
      if (!Directory.Exists(appDir + "/Data")) // built in bin/{debug/release}/{version}/
      {
        appDir = appDir + "/../../..";
      }

      _groupChatId = long.Parse(System.IO.File.ReadAllText(appDir + "/Data/telegram_bot_group_chat_id.json").Trim());


      string telegram_bot_token = System.IO.File.ReadAllText(appDir + "/Data/telegram_bot_token.json").Trim();
      string site_base_url = System.IO.File.ReadAllText(appDir + "/Data/site_base_url.json").Trim();


      Client = new TelegramBotClient(telegram_bot_token);

      if (!env.IsDevelopment())
      {
        Client.SetWebhookAsync("https://" + site_base_url + "/api/telegram").Wait();
      }
      else
      {
        _dev = true;
      }
    }

    
    public async Task SendMessageToGroupAsync(string msg)
    {
      if (_dev) return;

      await Client.SendTextMessageAsync(_groupChatId, msg);
    }
  }
}
