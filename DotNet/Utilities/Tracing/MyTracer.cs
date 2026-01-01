using System.Collections.Generic;
using System.Diagnostics;
using OpenTelemetry.Context.Propagation;

namespace MyTracing
{
    public static class MyTracer
    {
        private static readonly ActivitySource _activitySource = new(
            "my_service_activitysource", "1.0.0");

        public static Activity StartActivity(
            string name,
            ActivityKind kind = ActivityKind.Internal,
            PropagationContext propagationContext = default)
        {
            Activity activity = _activitySource.StartActivity(name, kind, propagationContext.ActivityContext);
            activity?.SetSuccess(false);
            if (activity is not null && Loggers.Tracing.IsTraceEnabled)
                Loggers.Tracing.Trace($"Started new activity '{activity.DisplayName}'");
            else if (activity is null && Loggers.Tracing.IsTraceEnabled)
                Loggers.Tracing.Trace($"Failed to start activity '{name}'");
            return activity;
        }

        public static Activity GetCurrentActivity()
        {
            if (Activity.Current?.Source == _activitySource)
                return Activity.Current;
            Loggers.Tracing.Error($"Failed to get current activity, current='{Activity.Current?.DisplayName}'");
            return null;
        }
}
