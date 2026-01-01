using System;
using System.Diagnostics;
using System.Linq;
using OpenTelemetry.Trace;

namespace MyTracing
{
    public static class ActivityExtensions
    {
        private const string DataPrefix = "data.";
        private const string ResultPrefix = "result.";

        public static void SetSuccess(
            this Activity activity,
            bool success)
        {
            if (success == true)
                activity.SetStatus(Status.Ok);
            else
                activity.SetStatus(Status.Error);
        }

        public static void AddError(
            this Activity activity,
            string message)
        {
            ActivityTagsCollection tags = new()
            {
                { "message", message },
            };

            activity?.AddEvent(new ActivityEvent("Error", default, tags));
        }

        public static void SetDataTag(
            this Activity activity,
            string key,
            object value)
        {
            activity.SetTag(DataPrefix + key, value);
        }

        public static void TrySetDataTag(
            this Activity activity,
            string key,
            Func<object> getValueFunc)
        {
            object value = null;
            try { value = getValueFunc(); } catch { }
            activity.SetDataTag(key, value);
        }

        public static void SetResultTag(
            this Activity activity,
            string key,
            object value)
        {
            activity.SetTag(ResultPrefix + key, value);
        }

        public static void TrySetResultTag(
            this Activity activity,
            string key,
            Func<object> getValueFunc)
        {
            object value = null;
            try { value = getValueFunc(); } catch { }
            activity.SetResultTag(key, value);
        }

        public static bool HasDataTagWithValue(
            this Activity activity,
            string key)
        {
            return activity.TagObjects.Any(x => x.Key == DataPrefix + key && x.Value is not null);
        }
    }
}
