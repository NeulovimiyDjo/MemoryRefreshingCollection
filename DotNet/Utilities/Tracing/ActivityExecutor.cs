using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;

namespace MyTracing
{
    public class ActivityExecutor
    {
        private readonly ILogger _logger;
        private readonly LogLevel _logLevel;
        private ActivityKind _activityKind;

        public ActivityExecutor(
            ILogger logger,
            LogLevel logLevel = null)
        {
            _logger = logger;
            _logLevel = logLevel ?? LogLevel.Info;
            _activityKind = ActivityKind.Internal;
        }

        public ActivityExecutor SetActivityKind(ActivityKind activityKind)
        {
            _activityKind = activityKind;
            return this;
        }

        public async Task TryDo(
            string activityName,
            Func<Activity, Task> executeActivityFunc,
            string activityDescriptionForLog = null,
            Func<object[]> getDescriptionParams = null,
            Func<Exception, Task> actionOnException = null)
        {
            try
            {
                await Do(activityName,
                    executeActivityFunc,
                    activityDescriptionForLog,
                    getDescriptionParams,
                    actionOnException);
            }
            catch { /*Ignore*/ }
        }

        public async Task<TResult> TryDo<TResult>(
            string activityName,
            Func<Activity, Task<TResult>> executeActivityFunc,
            string activityDescriptionForLog = null,
            Func<object[]> getDescriptionParams = null,
            Func<Exception, Task> actionOnException = null)
        {
            try
            {
                return await Do(activityName,
                    executeActivityFunc,
                    activityDescriptionForLog,
                    getDescriptionParams,
                    actionOnException);
            }
            catch { /*Ignore*/ }
            return default;
        }

        public async Task Do(
            string activityName,
            Func<Activity, Task> executeActivityFunc,
            string activityDescriptionForLog = null,
            Func<object[]> getDescriptionParams = null,
            Func<Exception, Task> actionOnException = null)
        {
            await Do(activityName, async activity =>
            {
                await executeActivityFunc(activity);
                return true;
            }, activityDescriptionForLog, getDescriptionParams, actionOnException);
        }

        public async Task<TResult> Do<TResult>(
            string activityName,
            Func<Activity, Task<TResult>> executeActivityFunc,
            string activityDescriptionForLog = null,
            Func<object[]> getDescriptionParams = null,
            Func<Exception, Task> actionOnException = null)
        {
            getDescriptionParams ??= () => Array.Empty<object>();

            LogActivityDescriptionAsStarted();
            using Activity activity = activityName is not null
                ? VtbTracer.StartActivity(activityName, _activityKind)
                : null;
            try
            {
                TResult result = await executeActivityFunc(activity);
                LogActivityDescriptionAsCompleted();
                activity?.SetSuccess(true);
                return result;
            }
            catch (Exception e)
            {
                if (actionOnException is not null)
                    await actionOnException(e);
                LogActivityDescriptionAsFailed(e);
                activity?.AddError(e.Message);
                throw;
            }

            void LogActivityDescriptionAsStarted()
            {
                if (activityDescriptionForLog is not null && LogEnabled())
                    _logger.Log(_logLevel, $"{activityDescriptionForLog} started", getDescriptionParams());
            }

            void LogActivityDescriptionAsCompleted()
            {
                if (activityDescriptionForLog is not null && LogEnabled())
                    _logger.Log(_logLevel, $"{activityDescriptionForLog} completed", getDescriptionParams());
            }

            void LogActivityDescriptionAsFailed(Exception e)
            {
                if (activityDescriptionForLog is not null)
                    _logger.Log(LogLevel.Error, e, $"{activityDescriptionForLog} failed:", getDescriptionParams());
                else
                    _logger.Log(LogLevel.Error, e, $"{activityName} failed:");
            }
        }

        private bool LogEnabled()
        {
            if (_logLevel == LogLevel.Off)
                return false;
            if (!_logger.IsTraceEnabled)
                return _logLevel > LogLevel.Trace;
            if (!_logger.IsDebugEnabled)
                return _logLevel > LogLevel.Debug;
            return true;
        }
    }
}
