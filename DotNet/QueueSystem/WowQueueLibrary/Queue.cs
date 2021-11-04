using System;
using System.Collections.Generic;
using System.Text;

namespace WowQueueLibrary
{
  public partial class Queue
  {
    public Dictionary<int,float> MatchExpectancies { get; set; }

    public Queue()
    {
      MatchExpectancies = new Dictionary<int, float>();

      Healers = new List<Healer>();
      DamageDealers = new List<DamageDealer>();

      ViableMatches = new List<Match>();
      ViableMatchesIds = new List<int>();
    }

    public Match FindMatch()
    {
      TryFindViableMatches();

      if (ViableMatches.Count > 0)
      {
        Random rand = new Random();
        int randomIndex = rand.Next(0, ViableMatches.Count - 1);
        Match match = ViableMatches[randomIndex];

        RemovePlayersFromQueue(match);

        return match;
      }
      else
      {
        return null;
      }
    }

    public void Print()
    {
      Console.WriteLine(" Healers:");
      foreach (var heal in Healers)
      {
        Console.WriteLine($"  {heal}");
      }

      Console.WriteLine("\n DamageDealers:");
      foreach (var dd in DamageDealers)
      {
        Console.WriteLine($"  {dd}");
      }
    }

    public void Clear()
    {
      Healers.Clear();
      DamageDealers.Clear();
    }

  }
}
