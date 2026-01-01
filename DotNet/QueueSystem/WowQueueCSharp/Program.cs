using System;
using WowQueueLibrary;

namespace WowQueueApp
{
  class Program
  {
    static void Main(string[] args)
    {
      Queue que = new Queue();
      que.GenerateExpectencyDictionaryExample();

      Console.WriteLine("--Avaliable commands: add show find exit\n");

      bool shouldExit = false;
      while (!shouldExit)
      {
        Console.Write("->");
        string command = Console.ReadLine();

        switch (command)
        {
          case "add":
            que.AddPlayersExamples();
            break;
          case "add5":
            for (int i = 0; i < 5; i++)
            {
              que.AddPlayersExamples();
            }
            break;
          case "show":
            que.Print();
            break;
          case "find":
            FindAndPlay(que);
            break;
          case "find9":
            for (int i = 0; i < 9; i++)
            {
              FindAndPlay(que);
            }
            break;
          case "clear":
            que.Clear();
            break;
          case "exit":
            shouldExit = true;
            break;
          default:
            Console.WriteLine("--command not found!");
            break;
        }
      }
    }

    static void FindAndPlay(Queue que)
    {
      Match match = que.FindMatch();
      if (match != null)
      {
        Console.WriteLine($"{match.ToString()}; e={match.Expectancy}; t1WinChance = {match.WinChanceOfT1}");
        Play(match, que);
      }
      else
      {
        Console.WriteLine(" No matches were found.");
      }
    }

    static void Play(Match match, Queue que)
    {
      Random rand = new Random();
      int p = rand.Next(14, 86);
      que.MatchExpectancies[match.Id] = p*0.01f;
    }
  }
}
