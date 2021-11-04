using System;
using System.Collections.Generic;
using System.Text;

namespace WowQueueLibrary
{
  internal class Constants
  {
    public const int RangeStep = 100;
    public const int DiffrentMatchesRequiredToGetQueue = 3;

    public const float InitialMaxExpectancyDiviationFrom05 = 0.1f;
    public const float MaxMaxExpectancyDiviationFrom05 = 0.37f;
    public const float MaxExpectancyDiviationFrom05Step = 0.05f;

    public const float EloRatingDevisor = 150.0f;
  }
}
