using System;
using System.Collections.Generic;
using System.Text;

namespace WowQueueLibrary
{
  internal enum Classes: byte { Warrior, DeathKnight, Paladin, Shaman, Hunter, Rogue, Druid, Warlock, Mage, Priest }
  internal enum HealerSpecs: byte { DiscPriest, HolyPaladin, RestoDruid, RestoShaman, HolyPriest, ProtoHealPaladin }
  internal enum DamageDealerSpecs : byte { ShadowPriest, ProtPaladin, RetriPaladin, FeralDruid, MoonkinDruid, ElementalShaman,
    EnhanceShaman, ArmsWarrior, ProtWarrior, FuryWarrior, UnholyDK, FrostDK, BloodDK, MmHunter, SurvivalHunter, BmHunter,
    SdRogue, MutiRogue, CombatRogue, FrostMage, FireMage, ArcaneMage, AffliWarlock, DemoWarlock, DestroWarlock }

  internal abstract class Player
  {
    public string Name { get; set; }
    public Classes Class { get; set; }

    public int Rating { get; set; }
    public int MaxMmrDrop { get; set; }

    public int UpperMmrLimit
    {
      get { return Rating + Constants.RangeStep; }
    }

    public int LowerMmrLimit
    {
      get { return Rating - Constants.RangeStep * MaxMmrDrop; }
    }

    public override string ToString()
    {
      return Name;
    }
  }

  internal class Healer: Player
  {
    public HealerSpecs Spec { get; set; }
  }

  internal class DamageDealer : Player
  {
    public DamageDealerSpecs Spec { get; set; }
  }

}
