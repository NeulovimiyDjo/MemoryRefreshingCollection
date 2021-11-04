using System;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBotTest
{
  class Program
  {
    private static TelegramBotClient client;

    static void Main(string[] args)
    {
      WebProxy wp = new WebProxy("150.95.109.143", true);

      //client = new TelegramBotClient("TTT", wp);
      client = new TelegramBotClient("TTT", wp);
      client.OnMessage += BotOnMessageReceived;
      client.OnMessageEdited += BotOnMessageReceived;
      client.StartReceiving();
      Console.ReadLine();
      client.StopReceiving();
    }

    private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
    {
      var message = messageEventArgs.Message;
      if (message?.Type == MessageType.Text)
      {
        await client.SendTextMessageAsync(message.Chat.Id, message.Text + "  sukaEBALVRONRNK");
      }
    }
  }
}
