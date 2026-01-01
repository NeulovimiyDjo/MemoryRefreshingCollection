using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTelemetry.Context.Propagation;

namespace MyTracing
{
    public static class ActivityPropagator
    {
        public static void Inject(Activity activity, Action<string> injectFunc)
        {
            if (activity is null)
                return;

            Dictionary<string, string> props = new();
            Propagators.DefaultTextMapPropagator.Inject(
                new PropagationContext(activity.Context, default),
                props,
                (props, key, value) =>
                {
                    props[key] = value;
                });

            string propsStr = System.Text.Json.JsonSerializer.Serialize(props);
            injectFunc(propsStr);
        }

        public static PropagationContext Extract(string propsStr)
        {
            if (propsStr is null)
                Loggers.Tracing.Warn($"Propagation context extraction called on empty props");
            if (propsStr is null)
                return default;

            var props = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(propsStr);
            var propagationContext = Propagators.DefaultTextMapPropagator.Extract(
                default,
                props,
                (props, key) =>
                {
                    if (props.ContainsKey(key))
                        return new[] { props[key] };
                    else
                        return new string[0];
                });

            if (Loggers.Tracing.IsTraceEnabled)
                Loggers.Tracing.Trace($"Extracted activity '{propagationContext.GetDisplayStr()}'");
            return propagationContext;
        }

        private static string GetDisplayStr(this PropagationContext pc)
        {
            ActivityContext ac = pc.ActivityContext;
            return $"{ac.TraceId}-{ac.SpanId}-{ac.IsRemote}-{ac.TraceFlags}-{ac.TraceState}";
        }
    }
}
