using System;
using System.Collections.Generic;
using System.Text;

namespace WowQueueLibrary
{
  public class Match
  {
    internal Team Team1 { get; set; }
    internal Team Team2 { get; set; }
    public int Id { get; set; }
    public float Expectancy { get; set; }

    public float WinChanceOfT1
    {
      get
      {
        if (WinChanceOfT1RatingOnly * Expectancy != 0)
          return 1 / (1 + (1 - WinChanceOfT1RatingOnly) * (1 - Expectancy) / (WinChanceOfT1RatingOnly * Expectancy));
        else
          return 0;
      }
    }

    private float WinChanceOfT1RatingOnly
    {
      get
      {
        return (float)(1 / (1 + Math.Pow(2, (Team2.AverageRating - Team1.AverageRating) / Constants.EloRatingDevisor)));
      }
    }

    public override string ToString()
    {
      return $" {Team1.DamageDealer1.Name} {Team1.DamageDealer2.Name} {Team1.Healer.Name} vs {Team2.DamageDealer1.Name} {Team2.DamageDealer2.Name} {Team2.Healer.Name}; Id = {Id}";
    }
  }
}
