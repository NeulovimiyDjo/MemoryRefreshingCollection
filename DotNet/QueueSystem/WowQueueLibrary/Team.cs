using System;
using System.Collections.Generic;
using System.Text;

namespace WowQueueLibrary
{
  internal class Team
  {
    public DamageDealer DamageDealer1 { get; set; }
    public DamageDealer DamageDealer2 { get; set; }
    public Healer Healer{ get; set; }

    public int AverageRating
    {
      get
      {
        return (int)Math.Round((DamageDealer1.Rating + DamageDealer2.Rating + Healer.Rating) / 3.0f);
      }
    }
  }
}
