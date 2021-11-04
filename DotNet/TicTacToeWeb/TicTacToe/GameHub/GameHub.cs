using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TicTacToe.GameHub
{
  public class Player
  {
    internal Dictionary<string, Settings> InvitedPlayersIds { get; set; } = new Dictionary<string, Settings>();
    internal List<string> InvitedByIds { get; set; } = new List<string>();
    internal Game Game { get; set; }
  }

  public class GameHub : Hub
  {
    public static object _lock = new object();
    public static Dictionary<string, Player> AvaliablePlayers = new Dictionary<string, Player>();
    private static Dictionary<string, Player> PlayingPlayers = new Dictionary<string, Player>();


    public async Task SendMove(int index)
    {
      var game = PlayingPlayers[Context.ConnectionId].Game;

      // check senders id if it's really his turn
      string currentMovePlayerId = game.GetNextMovePlayerId();
      if (Context.ConnectionId != currentMovePlayerId)
        return;

      bool wasValidMove = game.TryMakeMove(index);

      if (!wasValidMove)
        return;

      await Clients.Client(game.GetNextMovePlayerId()).SendAsync("MoveRecieved", index);

      if (game.GameOver)
      {
        await Clients.Client(Context.ConnectionId).SendAsync("GameEnded", game.GameOverConditions);
        await Clients.Client(game.GetNextMovePlayerId()).SendAsync("GameEnded", game.GameOverConditions);
      }
    }


    public async Task SendInviteRequest(string inviteeId, Settings settings)
    {
      string inviterId = Context.ConnectionId;

      if (inviteeId != inviterId && Game.SettingsAreValid(settings))
      {
        bool added = false;
        lock (_lock)
        {
          if (AvaliablePlayers.ContainsKey(inviterId) && AvaliablePlayers.ContainsKey(inviteeId) &&
              !AvaliablePlayers[inviterId].InvitedPlayersIds.ContainsKey(inviteeId))
          {
            AvaliablePlayers[inviterId].InvitedPlayersIds.Add(inviteeId, settings);
            AvaliablePlayers[inviteeId].InvitedByIds.Add(inviterId);
            added = true;
          }
        }

        if (added)
        {
          await Clients.Client(inviteeId).SendAsync("InviteRequested", inviterId, settings);
        }
      }
    }

    public async Task SendInviteDecline(string inviterId)
    {
      string inviteeId = Context.ConnectionId;

      bool removed = false;
      lock (_lock)
      {
        if (AvaliablePlayers.ContainsKey(inviterId) && AvaliablePlayers.ContainsKey(inviteeId))
        {
          removed = AvaliablePlayers[inviterId].InvitedPlayersIds.Remove(inviteeId);
          AvaliablePlayers[inviteeId].InvitedByIds.Remove(inviterId);
        }
      }

      if (removed)
      {
        await Clients.Client(inviteeId).SendAsync("InviteRemoved", inviterId);
      }
    }

    public async Task SendInviteAccept(string inviterId)
    {
      string inviteeId = Context.ConnectionId;

      Game game = null;

      lock (_lock)
      {
        if (AvaliablePlayers.ContainsKey(inviterId) && AvaliablePlayers.ContainsKey(inviteeId) &&
            AvaliablePlayers[inviterId].InvitedPlayersIds.ContainsKey(inviteeId))
        {
          // remove inviter from other avaliable players Invited and InvitedBy lists
          foreach (var id in AvaliablePlayers[inviterId].InvitedByIds)
          {
            if (id != inviterId && id != inviteeId && AvaliablePlayers.ContainsKey(id))
            {
              AvaliablePlayers[id].InvitedPlayersIds.Remove(inviterId);
              AvaliablePlayers[id].InvitedByIds.Remove(inviterId);
            }
          }
          // remove invitee from other avaliable players Invited and InvitedBy lists
          foreach (var id in AvaliablePlayers[inviteeId].InvitedByIds)
          {
            if (id != inviterId && id != inviteeId && AvaliablePlayers.ContainsKey(id))
            {
              AvaliablePlayers[id].InvitedPlayersIds.Remove(inviteeId);
              AvaliablePlayers[id].InvitedByIds.Remove(inviteeId);
            }
          }

          var settings = AvaliablePlayers[inviterId].InvitedPlayersIds[inviteeId];

          AvaliablePlayers.Remove(inviterId);
          AvaliablePlayers.Remove(inviteeId);


          game = Game.CreateGame(inviterId, inviteeId, settings);

          var inviter = new Player();
          var invitee = new Player();
          inviter.Game = game;
          invitee.Game = game;

          PlayingPlayers.Add(inviterId, inviter);
          PlayingPlayers.Add(inviteeId, invitee);
        }
      }

      if (game != null)
      {
        await Groups.RemoveFromGroupAsync(inviterId, "AvaliablePlayers");
        await Groups.RemoveFromGroupAsync(inviteeId, "AvaliablePlayers");

        await Clients.GroupExcept("AvaliablePlayers", inviterId, inviteeId).SendAsync("InviteRemoved", inviterId);
        await Clients.GroupExcept("AvaliablePlayers", inviterId, inviteeId).SendAsync("InviteRemoved", inviteeId);

        await Clients.GroupExcept("AvaliablePlayers", inviterId, inviteeId).SendAsync("AvaliablePlayersUpdtated", AvaliablePlayers.ToArray());

        await Clients.Clients(inviterId, inviteeId).SendAsync("GameCreated", game.CrossesId, game.NoughtsId, game.GameSettings);
      }
    }


    public async Task SendReplayRequest()
    {
      var currentId = Context.ConnectionId;

      Game game = null;
      string opponentId = "";

      bool opponentAlreadyLeft = true;

      var currentPlayer = PlayingPlayers[currentId];
      if (currentPlayer != null)
      {
        game = currentPlayer.Game;
        opponentId = game.GetOpponentId(currentId);

        if (PlayingPlayers.ContainsKey(opponentId))
          opponentAlreadyLeft = false;
      }

      if (game != null && !opponentAlreadyLeft)
      {
        bool forcedGameOver = game.FinishGame(currentId, "surrender");
        if (forcedGameOver)
        {
          var conditions = game.GameOverConditions;
          await Clients.Client(currentId).SendAsync("GameEnded", conditions);
          await Clients.Client(opponentId).SendAsync("GameEnded", conditions);
        }

        await Clients.Client(opponentId).SendAsync("ReplayRequested");
      }
    }

    public async Task SendReplayAccept()
    {
      var currentId = Context.ConnectionId;

      Game game = null;
      string opponentId = "";

      bool created = false;
      lock (_lock)
      {
        if (PlayingPlayers.ContainsKey(currentId))
        {
          game = PlayingPlayers[currentId].Game;
          opponentId = game.GetOpponentId(currentId);

          if (PlayingPlayers.ContainsKey(opponentId))
          {
            var settings = game.GameSettings;
            game = Game.CreateGame(opponentId, currentId, settings);

            PlayingPlayers[currentId].Game = game;
            PlayingPlayers[opponentId].Game = game;

            created = true;
          }
        }
      }

      if (created)
      {
        await Clients.Clients(currentId, opponentId).SendAsync("GameCreated", game.CrossesId, game.NoughtsId, game.GameSettings);
      }
    }

    public async Task SendReplayDecline()
    {
      var currentId = Context.ConnectionId;

      string opponentId = "";

      var currentPlayer = PlayingPlayers[currentId];
      if (currentPlayer != null)
      {
        opponentId = currentPlayer.Game.GetOpponentId(currentId);

        if (PlayingPlayers.ContainsKey(opponentId))
        {
          await Clients.Client(opponentId).SendAsync("ReplayDeclined");
        }
      }
    }


    public async Task ResumeGameSearch()
    {
      var currentId = Context.ConnectionId;

      Game game = null;
      string opponentId = "";

      lock (_lock)
      {
        if (PlayingPlayers.ContainsKey(currentId))
        {
          game = PlayingPlayers[currentId].Game;
          opponentId = game.GetOpponentId(currentId);

          PlayingPlayers.Remove(currentId);
        }

        AvaliablePlayers.Add(currentId, new Player());
      }

      if (game != null)
      {
        bool forcedGameOver = game.FinishGame(currentId, "exit");
        if (forcedGameOver)
        {
          var conditions = game.GameOverConditions;
          await Clients.Client(opponentId).SendAsync("GameEnded", conditions);
        }

        await Clients.Client(opponentId).SendAsync("OpponentExited");
      }

      await Groups.AddToGroupAsync(currentId, "AvaliablePlayers");

      await Clients.Caller.SendAsync("MyIdAndAvaliablePlayers", currentId, AvaliablePlayers.ToArray());
      await Clients.GroupExcept("AvaliablePlayers", Context.ConnectionId).SendAsync("AvaliablePlayersUpdtated", AvaliablePlayers.ToArray());
    }

    public override async Task OnConnectedAsync()
    {
      lock(_lock)
      {
        AvaliablePlayers.Add(Context.ConnectionId, new Player());
      }

      await Groups.AddToGroupAsync(Context.ConnectionId, "AvaliablePlayers");


      await Clients.Caller.SendAsync("MyIdAndAvaliablePlayers", Context.ConnectionId, AvaliablePlayers.ToArray());
      await Clients.GroupExcept("AvaliablePlayers", Context.ConnectionId).SendAsync("AvaliablePlayersUpdtated", AvaliablePlayers.ToArray());


      await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      var currentId = Context.ConnectionId;

      Game game = null;
      string opponentId = "";

      lock (_lock)
      {
        if (PlayingPlayers.ContainsKey(currentId))
        {
          game = PlayingPlayers[currentId].Game;
          opponentId = game.GetOpponentId(currentId);

          PlayingPlayers.Remove(currentId);
        }

        AvaliablePlayers.Remove(currentId);
      }

      if (game != null)
      {
        bool forcedGameOver = game.FinishGame(currentId, "disconnect");
        if (forcedGameOver)
        {
          var conditions = game.GameOverConditions;
          await Clients.Client(opponentId).SendAsync("GameEnded", conditions);
        }

        await Clients.Client(opponentId).SendAsync("OpponentExited");
      }


      await Groups.RemoveFromGroupAsync(currentId, "AvaliablePlayers");

      await Clients.GroupExcept("AvaliablePlayers", currentId).SendAsync("InviteRemoved", currentId);
      await Clients.GroupExcept("AvaliablePlayers", currentId).SendAsync("AvaliablePlayersUpdtated", AvaliablePlayers.ToArray());


      await base.OnDisconnectedAsync(exception);
    }
  }
}
