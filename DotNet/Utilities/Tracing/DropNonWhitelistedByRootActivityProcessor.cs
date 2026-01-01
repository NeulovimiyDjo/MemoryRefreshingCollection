using System.Collections.Generic;
using System.Diagnostics;
using OpenTelemetry;

namespace MyTracing
{
    public class DropNonWhitelistedByRootActivityProcessor : BaseProcessor<Activity>
    {
        private readonly HashSet<string> _displayNames;

        public DropNonWhitelistedByRootActivityProcessor(IEnumerable<string> displayNames)
        {
            _displayNames = new(displayNames);
        }

        public override void OnEnd(Activity data)
        {
            Activity rootActivity = GetRootActivity(data);
            if (!_displayNames.Contains(rootActivity.DisplayName)
                && data.ActivityTraceFlags == ActivityTraceFlags.Recorded)
            {
                data.ActivityTraceFlags = ActivityTraceFlags.None;
                if (Loggers.Tracing.IsTraceEnabled)
                    Loggers.Tracing.Trace($"Dropped non whitelisted by root activity '{data.DisplayName}', root='{rootActivity.DisplayName}'");
            }
        }

        public Activity GetRootActivity(Activity activity)
        {
            Activity current = activity;
            while (current.Parent is not null)
                current = current.Parent;
            return current;
        }
    }
}
