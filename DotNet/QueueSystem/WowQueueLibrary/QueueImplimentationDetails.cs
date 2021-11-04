using System;
using System.Collections.Generic;
using System.Text;

namespace WowQueueLibrary
{
  public partial class Queue
  {
    internal List<Healer> Healers { get; set; }
    internal List<DamageDealer> DamageDealers { get; set; }

    private static readonly int ddSpecsCount = Enum.GetNames(typeof(DamageDealerSpecs)).Length;
    private static readonly int healSpecsCount = Enum.GetNames(typeof(HealerSpecs)).Length;

    private List<Match> ViableMatches { get; set; }
    private List<int> ViableMatchesIds { get; set; }
    private float MaxExpectancyDiviationFrom05 { get; set; }


    private void TryFindViableMatches()
    {
      ViableMatches.Clear();
      ViableMatchesIds.Clear();
      MaxExpectancyDiviationFrom05 = Constants.InitialMaxExpectancyDiviationFrom05;

      while ((ViableMatchesIds.Count < Constants.DiffrentMatchesRequiredToGetQueue) &&
        (MaxExpectancyDiviationFrom05 <= Constants.MaxMaxExpectancyDiviationFrom05))
      {
        CheckHealers();

        MaxExpectancyDiviationFrom05 += Constants.MaxExpectancyDiviationFrom05Step;
      }
    }

    private void RemovePlayersFromQueue(Match match)
    {
      Healers.Remove(match.Team1.Healer);
      Healers.Remove(match.Team2.Healer);
      DamageDealers.Remove(match.Team1.DamageDealer1);
      DamageDealers.Remove(match.Team1.DamageDealer2);
      DamageDealers.Remove(match.Team2.DamageDealer1);
      DamageDealers.Remove(match.Team2.DamageDealer2);
    }

    private void CheckHealers()
    {
      // represents combinations of healers of each spec we checked (default value is false)
      bool[] combinationsChecked = new bool[healSpecsCount * healSpecsCount];

      // [h1,h2] goes like [1,0]  [2,0] [2,1]  [3,0] [3,1] [3,2] and so on
      for (int h1 = 1; h1 < Healers.Count; h1++)
        for (int h2 = 0; h2 < h1; h2++)
          if (
            !combinationsChecked[(byte)Healers[h1].Spec * healSpecsCount + (byte)Healers[h2].Spec] &&
            Healers[h1].UpperMmrLimit >= Healers[h2].LowerMmrLimit && Healers[h1].LowerMmrLimit <= Healers[h2].UpperMmrLimit)
          {
            // combinations are insensitive/symmetric to order
            combinationsChecked[(byte)Healers[h1].Spec * healSpecsCount + (byte)Healers[h2].Spec] = true;
            combinationsChecked[(byte)Healers[h2].Spec * healSpecsCount + (byte)Healers[h1].Spec] = true;

            CheckDamageDealers(Healers[h1], Healers[h2]);
            if (ViableMatchesIds.Count >= Constants.DiffrentMatchesRequiredToGetQueue) break;
          }
    }

    private void CheckDamageDealers(Healer h1, Healer h2)
    {
      // represents how many dds of each spec we checked since there's no point to check more than 2 players of each spec
      int[] specChecksAmount = new int[ddSpecsCount];

      var viableDamageDealers = new List<DamageDealer>();
      for (int i = 0; i < DamageDealers.Count; i++)
      {
        // decrease how many specs we should check if healer already took 1 spot from all specs of his class
        int specChecksLimit = 2;
        if (DamageDealers[i].Class == h1.Class) specChecksLimit--;
        if (DamageDealers[i].Class == h2.Class) specChecksLimit--;

        if (
          specChecksAmount[(byte)DamageDealers[i].Spec] < specChecksLimit &&
          DamageDealers[i].UpperMmrLimit >= Math.Max(h1.LowerMmrLimit, h1.LowerMmrLimit) &&
          DamageDealers[i].LowerMmrLimit <= Math.Min(h1.UpperMmrLimit, h2.UpperMmrLimit))
        {
          specChecksAmount[(byte)DamageDealers[i].Spec]++;
          viableDamageDealers.Add(DamageDealers[i]);
        }

        // if we checked 2 players of each spec - 2 taken by healers there's no point to go further, we already have enough for every possible combination
        if (viableDamageDealers.Count >= 2 * ddSpecsCount - 2) break;
      }

      if (viableDamageDealers.Count >= 4)
      {
        SearchViableMatches(viableDamageDealers, h1, h2);
      }
    }

    private void SearchViableMatches(List<DamageDealer> dds, Healer h1, Healer h2)
    {
      // [dd1,dd2,dd3,dd4] goes like [0,1,2,3]  [0,1,2,4] [0,1,3,4] [0,2,3,4] [1,2,3,4]  [0,1,2,5] [0,1,3,5] [0,2,3,5] [1,2,3,5] [0,1,4,5] [0,2,4,5] [1,2,4,5] [0,3,4,5] [1,3,4,5] [2,3,4,5] and so on
      for (int dd4 = 3; dd4 < dds.Count; dd4++)
        for (int dd3 = 2; dd3 < dd4; dd3++)
          for (int dd2 = 1; dd2 < dd3; dd2++)
            for (int dd1 = 0; dd1 < dd2; dd1++)
            {
              AnalyzeAllMatches(dds[dd1], dds[dd2], dds[dd3], dds[dd4], h1, h2);

              if (ViableMatchesIds.Count >= Constants.DiffrentMatchesRequiredToGetQueue) return;
            }
    }

    private void AnalyzeAllMatches(DamageDealer dd1, DamageDealer dd2, DamageDealer dd3, DamageDealer dd4, Healer h1, Healer h2)
    {
      AnalyzeMatch(dd1, dd2, h1, dd3, dd4, h2);
      AnalyzeMatch(dd1, dd2, h2, dd3, dd4, h1);

      AnalyzeMatch(dd1, dd3, h1, dd2, dd4, h2);
      AnalyzeMatch(dd1, dd3, h2, dd2, dd4, h1);

      AnalyzeMatch(dd1, dd4, h1, dd2, dd3, h2);
      AnalyzeMatch(dd1, dd4, h2, dd2, dd3, h1);
    }

    private void AnalyzeMatch(DamageDealer dd1, DamageDealer dd2, Healer h1, DamageDealer dd3, DamageDealer dd4, Healer h2)
    {
      if (dd1.Class == dd2.Class || dd1.Class == h1.Class || dd2.Class == h1.Class ||
        dd3.Class == dd4.Class || dd3.Class == h2.Class || dd4.Class == h2.Class) return;

      (int matchId, bool revertTeamsOrder) = MatchToInt(dd1, dd2, h1, dd3, dd4, h2);

      if (!MatchExpectancies.TryGetValue(matchId, out float e))
        e = 0.5f; // even chances for unknown combs

      if (!ViableMatchesIds.Contains(matchId) && Math.Abs(e - 0.5f) <= MaxExpectancyDiviationFrom05)
      {
        ViableMatchesIds.Add(matchId);

        if (!revertTeamsOrder) // first team id is always bigger in match id calculation (to be able to get correct expectency for ratings)
        {
          ViableMatches.Add(new Match
          {
            Team1 = new Team { DamageDealer1 = dd1, DamageDealer2 = dd2, Healer = h1 },
            Team2 = new Team { DamageDealer1 = dd3, DamageDealer2 = dd4, Healer = h2 },
            Id = matchId,
            Expectancy = e
          });
        }
        else
        {
          ViableMatches.Add(new Match
          {
            Team2 = new Team { DamageDealer1 = dd1, DamageDealer2 = dd2, Healer = h1 },
            Team1 = new Team { DamageDealer1 = dd3, DamageDealer2 = dd4, Healer = h2 },
            Id = matchId,
            Expectancy = e
          });
        }
      }
    }

    private (int, bool) MatchToInt(DamageDealer dd1, DamageDealer dd2, Healer h1, DamageDealer dd3, DamageDealer dd4, Healer h2)
    {
      int t1Id, t2Id, res;

      if (dd1.Spec > dd2.Spec)
        t1Id = (int)dd1.Spec + (int)dd2.Spec * ddSpecsCount + (int)h1.Spec * ddSpecsCount * ddSpecsCount;
      else
        t1Id = (int)dd2.Spec + (int)dd1.Spec * ddSpecsCount + (int)h1.Spec * ddSpecsCount * ddSpecsCount;

      if (dd3.Spec > dd4.Spec)
        t2Id = (int)dd3.Spec + (int)dd4.Spec * ddSpecsCount + (int)h2.Spec * ddSpecsCount * ddSpecsCount;
      else
        t2Id = (int)dd4.Spec + (int)dd3.Spec * ddSpecsCount + (int)h2.Spec * ddSpecsCount * ddSpecsCount;


      bool revertTeamsOrder;
      if (t1Id > t2Id)
        (res, revertTeamsOrder) = (t1Id + t2Id * healSpecsCount * ddSpecsCount * ddSpecsCount, false);
      else
        (res, revertTeamsOrder) = (t2Id + t1Id * healSpecsCount * ddSpecsCount * ddSpecsCount, true);

      return (res, revertTeamsOrder);
    }

  }
}
