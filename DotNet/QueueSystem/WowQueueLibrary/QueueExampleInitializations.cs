using System;
using System.Collections.Generic;
using System.Text;

namespace WowQueueLibrary
{
  public partial class Queue
  {
    public void AddPlayersExamples()
    {
      Healers.AddRange(new Healer[] {
        new Healer { Name = "3029Hpal", Class = Classes.Paladin, Spec = HealerSpecs.HolyPaladin, Rating = 3029, MaxMmrDrop = 1 },
        new Healer { Name = "2942Hpal", Class = Classes.Paladin, Spec = HealerSpecs.HolyPaladin, Rating = 2942, MaxMmrDrop = 1 },
        new Healer { Name = "3011Dpri", Class = Classes.Priest, Spec = HealerSpecs.DiscPriest, Rating = 3011, MaxMmrDrop = 1 },
        new Healer { Name = "3142Dpri", Class = Classes.Priest, Spec = HealerSpecs.DiscPriest, Rating = 3142, MaxMmrDrop = 2 },
      });

      DamageDealers.AddRange(new DamageDealer[] {
        new DamageDealer { Name = "3073Awar", Class = Classes.Warrior, Spec = DamageDealerSpecs.ArmsWarrior, Rating = 3073, MaxMmrDrop = 1 },
        new DamageDealer { Name = "2927Awar", Class = Classes.Warrior, Spec = DamageDealerSpecs.ArmsWarrior, Rating = 2927, MaxMmrDrop = 1 },
        new DamageDealer { Name = "3021Udk", Class = Classes.DeathKnight, Spec = DamageDealerSpecs.UnholyDK, Rating = 3021, MaxMmrDrop = 1 },
        new DamageDealer { Name = "3105Udk", Class = Classes.DeathKnight, Spec = DamageDealerSpecs.UnholyDK, Rating = 3105, MaxMmrDrop = 2 },
        new DamageDealer { Name = "3001Ele", Class = Classes.Shaman, Spec = DamageDealerSpecs.ElementalShaman, Rating = 3001, MaxMmrDrop = 2 },
        new DamageDealer { Name = "3175Ele", Class = Classes.Shaman, Spec = DamageDealerSpecs.ElementalShaman, Rating = 3175, MaxMmrDrop = 3 },
        new DamageDealer { Name = "2888Afli", Class = Classes.Warlock, Spec = DamageDealerSpecs.AffliWarlock, Rating = 2888, MaxMmrDrop = 1 },
        new DamageDealer { Name = "3072Spri", Class = Classes.Priest, Spec = DamageDealerSpecs.ShadowPriest, Rating = 3072, MaxMmrDrop = 2 },
      });
    }

    public void GenerateExpectencyDictionaryExample()
    {
      Healer hPal = new Healer { Name = "", Class = Classes.Paladin, Spec = HealerSpecs.HolyPaladin, Rating = 1, MaxMmrDrop = 1 };
      Healer dPri = new Healer { Name = "", Class = Classes.Priest, Spec = HealerSpecs.DiscPriest, Rating = 1, MaxMmrDrop = 1 };

      DamageDealer aWar = new DamageDealer { Name = "", Class = Classes.Warrior, Spec = DamageDealerSpecs.ArmsWarrior, Rating = 1, MaxMmrDrop = 1 };
      DamageDealer uDk = new DamageDealer { Name = "", Class = Classes.DeathKnight, Spec = DamageDealerSpecs.UnholyDK, Rating = 1, MaxMmrDrop = 1 };
      DamageDealer ele = new DamageDealer { Name = "", Class = Classes.Shaman, Spec = DamageDealerSpecs.ElementalShaman, Rating = 1, MaxMmrDrop = 1 };
      DamageDealer sPri = new DamageDealer { Name = "", Class = Classes.Priest, Spec = DamageDealerSpecs.ShadowPriest, Rating = 1, MaxMmrDrop = 1 };
      DamageDealer afli = new DamageDealer { Name = "", Class = Classes.Warlock, Spec = DamageDealerSpecs.AffliWarlock, Rating = 1, MaxMmrDrop = 1 };


      // during dictionary update revert expectency of Team1 winning if revertTeamsOrder is true
      (int matchId, bool revertTeamsOrder) = MatchToInt(aWar, uDk, hPal, aWar, uDk, dPri);
      float e = revertTeamsOrder ? 1 - 0.8f : 0.8f;
      MatchExpectancies[matchId] = e;

      (matchId, revertTeamsOrder) = MatchToInt(aWar, uDk, hPal, uDk, ele, hPal);
      e = revertTeamsOrder ? 1 - 0.6f : 0.6f;
      MatchExpectancies[matchId] = e;

      (matchId, revertTeamsOrder) = MatchToInt(aWar, ele, hPal, aWar, uDk, dPri);
      e = revertTeamsOrder ? 1 - 0.66f : 0.62f;
      MatchExpectancies[matchId] = e;

      (matchId, revertTeamsOrder) = MatchToInt(afli, sPri, hPal, aWar, uDk, hPal);
      e = revertTeamsOrder ? 1 - 0.71f : 0.71f;
      MatchExpectancies[matchId] = e;

      (matchId, revertTeamsOrder) = MatchToInt(afli, sPri, hPal, aWar, uDk, dPri);
      e = revertTeamsOrder ? 1 - 0.67f : 0.67f;
      MatchExpectancies[matchId] = e;

      (matchId, revertTeamsOrder) = MatchToInt(afli, aWar, dPri, aWar, uDk, hPal);
      e = revertTeamsOrder ? 1 - 0.22f : 0.22f;
      MatchExpectancies[matchId] = e;

      (matchId, revertTeamsOrder) = MatchToInt(aWar, uDk, hPal, afli, ele, dPri);
      e = revertTeamsOrder ? 1 - 0.35f : 0.35f;
      MatchExpectancies[matchId] = e;

      (matchId, revertTeamsOrder) = MatchToInt(aWar, ele, hPal, aWar, ele, dPri);
      e = revertTeamsOrder ? 1 - 0.58f : 0.58f;
      MatchExpectancies[matchId] = e;
    }
  }
}
