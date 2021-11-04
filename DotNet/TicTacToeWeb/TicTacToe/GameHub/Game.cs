using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace TicTacToe.GameHub
{
  public struct Settings
  {
    public int xDim;
    public int yDim;
    public int winSize;
    public int timeout;
  }

  public struct Conditions
  {
    public string winner;
    public string reason;
    public string direction;
    public int i;
    public int j;
  }

  public class Game
  {
    internal bool GameOver { get; set; } = false;

    internal Settings GameSettings { get; set; }
    internal Conditions GameOverConditions { get; set; }

    internal string CrossesId { get; set; }
    internal string NoughtsId { get; set; }


    internal static Game CreateGame(string player1Id, string player2Id, Settings settings)
    {
      Game game;

      Random rand = new Random();
      bool player1FirstMove = rand.Next(0, 2) == 0;

      if (player1FirstMove) // crosses = player1
      {
        game = new Game(player1Id, player2Id, settings);
      }
      else // crosses = player2
      {
        game = new Game(player2Id, player1Id, settings);
      }

      return game;
    }

    internal static bool SettingsAreValid(Settings settings)
    {
      if (settings.xDim < MinXDim || settings.yDim < MinYDim || settings.winSize < MinWinSize ||
          settings.xDim > MaxXDim || settings.yDim > MaxYDim || settings.winSize > MaxWinSize ||
          settings.winSize > Math.Min(settings.xDim, settings.yDim))
      {
        return false;
      }

      return true;
    }

    internal bool TryMakeMove(int index)
    {
      if (GameOver || Cells[index] != CellTypes.Clear)
        return false;

      if (MovesCount % 2 == 0)
      {
        Cells[index] = CellTypes.Cross;
      }
      else if (MovesCount % 2 == 1)
      {
        Cells[index] = CellTypes.Nought;
      }

      MovesCount++;

      CheckGame();

      if (GameOver)
        Timer.Dispose();
      else
        Timer.Change(TimeoutInMilliseconds, Timeout.Infinite);

      return true;
    }

    internal string GetNextMovePlayerId()
    {
      if (MovesCount % 2 == 0)
      {
        return CrossesId;
      }
      else
      {
        return NoughtsId;
      }
    }

    internal string GetOpponentId(string currId)
    {
      if (currId == CrossesId)
      {
        return NoughtsId;
      }
      else
      {
        return CrossesId;
      }
    }

    internal bool FinishGame(string looserId, string reason)
    {
      Timer.Dispose();

      if (!GameOver)
      {
        if (looserId == CrossesId)
        {
          Winner = "noughts";
        }
        else
        {
          Winner = "crosses";
        }

        GameOver = true;
        GameOverConditions = new Conditions { winner = Winner, reason = reason, direction = "none", i = -1, j = -1 };

        return true;
      }

      return false;
    }

    #region private fields/props
    private enum CellTypes : byte { Clear = 0, Cross, Nought }

    private const int MinXDim = 3;
    private const int MinYDim = 3;
    private const int MinWinSize = 2;
    private const int MaxXDim = 100;
    private const int MaxYDim = 100;
    private const int MaxWinSize = 30;


    private CellTypes[] Cells;

    private string Winner { get; set; } = "";
    private int MovesCount { get; set; } = 0;
    private int TimeoutInMilliseconds { get; set; }

    private readonly IHubClients Clients;
    private readonly Timer Timer;
    #endregion

    #region private methods
    private Game(string crossesId, string noughtsId, Settings settings)
    {
      CrossesId = crossesId;
      NoughtsId = noughtsId;

      GameSettings = settings;

      if (settings.timeout > 0 && settings.timeout < Math.Pow(2, 32 - 10))
        TimeoutInMilliseconds = settings.timeout * 1000;
      else
        TimeoutInMilliseconds = Timeout.Infinite;

      Cells = new CellTypes[GameSettings.xDim * GameSettings.yDim];

      Clients = Startup.ServiceProvider.GetService<IHubContext<GameHub>>().Clients;
      Timer = new Timer(HandleTimeout, null, TimeoutInMilliseconds, Timeout.Infinite);
    }

    private async void HandleTimeout(object state)
    {
      FinishGame(GetNextMovePlayerId(), "timeout");

      await Clients.Client(CrossesId).SendAsync("GameEnded", GameOverConditions);
      await Clients.Client(NoughtsId).SendAsync("GameEnded", GameOverConditions);
    }

    private string CalculateWinner()
    {
      if (MovesCount % 2 == 0)
      {
        return "noughts";
      }
      else
      {
        return "crosses";
      }
    }

    // ckeck for chain=|winSize cells of same type in a row| in 4 possible directions
    // for every valid starting point on the field
    private void CheckGame()
    {
      // check rows
      for (int i = 0; i < GameSettings.yDim; i++)
      {
        for (int j = 0; j < GameSettings.xDim - GameSettings.winSize + 1; j++)
        {

          CellTypes start = Cells[i * GameSettings.xDim + j];
          if (start != CellTypes.Clear)
          {

            bool foundChain = true;
            for (int k = 1; k < GameSettings.winSize; k++)
            {
              if (Cells[i * GameSettings.xDim + (j + k)] != start)
                foundChain = false;
            }

            if (foundChain)
            {
              Winner = CalculateWinner();
              GameOver = true;
              GameOverConditions = new Conditions { winner = Winner, direction = "right", i = i, j = j };
              return;
            }
          }
        }
      }

      // check columns
      for (int i = 0; i < GameSettings.yDim - GameSettings.winSize + 1; i++)
      {
        for (int j = 0; j < GameSettings.xDim; j++)
        {

          CellTypes start = Cells[i * GameSettings.xDim + j];
          if (start != CellTypes.Clear)
          {

            bool foundChain = true;
            for (int k = 1; k < GameSettings.winSize; k++)
            {
              if (Cells[(i + k) * GameSettings.xDim + j] != start)
                foundChain = false;
            }

            if (foundChain)
            {
              Winner = CalculateWinner();
              GameOver = true;
              GameOverConditions = new Conditions { winner = Winner, direction = "down", i = i, j = j };
              return;
            }
          }
        }
      }

      // check dioganal right+down
      for (int i = 0; i < GameSettings.yDim - GameSettings.winSize + 1; i++)
      {
        for (int j = 0; j < GameSettings.xDim - GameSettings.winSize + 1; j++)
        {

          CellTypes start = Cells[i * GameSettings.xDim + j];
          if (start != CellTypes.Clear)
          {

            bool foundChain = true;
            for (int k = 1; k < GameSettings.winSize; k++)
            {
              if (Cells[(i + k) * GameSettings.xDim + (j + k)] != start)
                foundChain = false;
            }

            if (foundChain)
            {
              Winner = CalculateWinner();
              GameOver = true;
              GameOverConditions = new Conditions { winner = Winner, direction = "right+down", i = i, j = j };
              return;
            }
          }
        }
      }

      // check dioganal left+down
      for (int i = 0; i < GameSettings.yDim - GameSettings.winSize + 1; i++)
      {
        for (int j = GameSettings.winSize - 1; j < GameSettings.xDim; j++)
        {

          CellTypes start = Cells[i * GameSettings.xDim + j];
          if (start != CellTypes.Clear)
          {

            bool foundChain = true;
            for (int k = 1; k < GameSettings.winSize; k++)
            {
              if (Cells[(i + k) * GameSettings.xDim + (j - k)] != start)
                foundChain = false;
            }

            if (foundChain)
            {
              Winner = CalculateWinner();
              GameOver = true;
              GameOverConditions = new Conditions { winner = Winner, direction = "left+down", i = i, j = j };
              return;
            }
          }
        }
      }

      if (MovesCount == GameSettings.xDim * GameSettings.yDim)
      {
        Winner = "draw";
        GameOver = true;
        GameOverConditions = new Conditions { winner = Winner, direction = "none", i = -1, j = -1 };
        return;
      }
    }
    #endregion
  }
}
