using NLog;
using System;
using System.Linq;
using System.Runtime.Serialization;

public static class StackTraceLogger
{
    private static Logger _logger = LogManager.GetLogger("STACK_TRACE");

    public static void Log(string msg, params object[] objects)
    {
        if (_logger.IsTraceEnabled)
        {
            try
            {
                string fullMsg = $"Message=[{msg}]";
                foreach (object obj in objects.Where(x => x is not null))
                {
                    fullMsg += $"\n\n{Jsonize(obj)}";
                }
                _logger.Trace($"{fullMsg}\n\nCallStack:\n{Environment.StackTrace}\n\n");
            }
            catch (Exception ex)
            {
                _logger.LogException($"ERROR WHILE WRITING TRYING TO LOG FOR MSG=[{msg}]", ex);
            }
        }
    }

    private static string Jsonize(object obj)
    {
        if (obj is string str)
            return str;
        else
            return JsonConvert.SerializeObject(
                obj,
                Formatting.Indented
            );
    }
}
