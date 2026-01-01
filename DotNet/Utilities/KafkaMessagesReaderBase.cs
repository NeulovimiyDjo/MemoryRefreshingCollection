using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Confluent.Kafka;
using NLog;

namespace KafkaMessagesReaderBaseProject;

public abstract class KafkaMessagesReaderBase
{
    protected abstract string Topic { get; }
    protected abstract int ConsumeTimeoutMs { get; }
    protected abstract KafkaConsumerConfigSettings ConfigSettings { get; }
    protected abstract OffsetsCommitStrategy CommitStrategy { get; }
    protected abstract string TryReadMessageActivityName { get; }

    protected abstract void LogSubscribedToTopic(string topic, ConsumerConfig config);
    protected abstract void LogReachedTheEndOfConsumerQueue(int handledCount);
    protected abstract void LogReadMessage(ConsumeResult<string, string> cr);
    protected abstract void LogProcessedMessage(ConsumeResult<string, string> cr);
    protected abstract void LogCommittedOffset(ConsumeResult<string, string> cr);
    protected abstract void LogCommittedAllOffsets();
    protected abstract void LogFailedToReadMessage(Exception e, ConsumeResult<string, string> lastCr, int handledCount);
    protected abstract void LogFailedToProcessMessage(Exception e, ConsumeResult<string, string> lastCr, int handledCount);
    protected abstract void LogFailedToCommitOffset(Exception e, ConsumeResult<string, string> lastCr, int handledCount);
    protected abstract void LogFailedToCommitAllOffsets(Exception e);

    public bool IsEnabled { get; set; } = true;
    public bool UseFakeConsumer { get; set; } = false;
    public Func<ConsumerConfig, IConsumer<string, string>> CreateFakeConsumerFunc { get; set; } = config =>
        throw new Exception("Create fake consumer function is not set");

    protected readonly ILogger _logger;

    protected KafkaMessagesReaderBase(ILogger logger)
    {
        _logger = logger;
    }

    protected async Task ReadAndProcessMessages(
        Func<ConsumeResult<string, string>, Task> processMessageFunc)
    {
        if (!IsEnabled)
            return;

        ConsumerConfig config = CreateConsumerConfig();
        using IConsumer<string, string> consumer = CreateConsumer(config);
        consumer.Subscribe(Topic);
        LogSubscribedToTopic(Topic, config);

        int handledCount = 0;
        ConsumeResult<string, string> cr = null;
        try
        {
            while (true)
            {
                cr = TryReadMessage(consumer);
                if (cr is null)
                {
                    LogReachedTheEndOfConsumerQueue(handledCount);
                    break;
                }
                LogReadMessage(cr);

                await ExecuteInTryCatchWithRethrow(async () =>
                {
                    await processMessageFunc(cr);
                }, onException: e => LogFailedToProcessMessage(e, cr, handledCount));
                LogProcessedMessage(cr);

                if (CommitStrategy == OffsetsCommitStrategy.AfterMessageProcessed)
                {
                    ExecuteInTryCatchWithRethrow(() =>
                    {
                        consumer.Commit(cr);
                    }, onException: e => LogFailedToCommitOffset(e, cr, handledCount));
                    LogCommittedOffset(cr);
                }

                handledCount++;
            }

            if (handledCount > 0 && CommitStrategy == OffsetsCommitStrategy.OnceAfterAllMessagesProcessed)
            {
                ExecuteInTryCatchWithRethrow(() =>
                {
                    consumer.Commit();
                }, onException: e => LogFailedToCommitAllOffsets(e));
                LogCommittedAllOffsets();
            }
        }
        catch
        {
            throw;
        }
        finally
        {
#pragma warning disable format
#pragma warning disable IDE2001 // Embedded statements must be on their own line
            try { consumer.Unsubscribe(); } catch (Exception e) { _logger.LogException($"Consumer failed to unsubscribe:", e); }
            try { consumer.Close(); } catch (Exception e) { _logger.LogException($"Consumer failed to close:", e); }
#pragma warning restore IDE2001 // Embedded statements must be on their own line
#pragma warning restore format
        }

        ConsumeResult<string, string> TryReadMessage(IConsumer<string, string> consumer)
        {
            using Activity activity = VtbTracer.StartActivity(TryReadMessageActivityName, ActivityKind.Consumer);
            return ExecuteInTryCatchWithRethrow(() =>
            {
                ConsumeResult<string, string> res = consumer.Consume(ConsumeTimeoutMs);
                activity?.SetResultTag(ActivitiesTags.TrySuccessful, res is not null);
                activity?.SetSuccess(true);
                return res;
            }, onException: e =>
            {
                LogFailedToReadMessage(e, cr, handledCount);
                activity?.AddError(e.Message);
            });
        }
    }

    private ConsumerConfig CreateConsumerConfig()
    {
        ConsumerConfig config = new()
        {
            BootstrapServers = ConfigSettings.BootstrapServers,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = CommitStrategy == OffsetsCommitStrategy.PeriodicallyMessagedRead,
            AutoCommitIntervalMs = ConfigSettings.AutoCommitIntervalMs ?? 0,
            SessionTimeoutMs = ConfigSettings.SessionTimeoutMs,
            GroupId = ConfigSettings.GroupId,
            ClientId = Dns.GetHostName(),
            Debug = !string.IsNullOrWhiteSpace(ConfigSettings.Debug) ? ConfigSettings.Debug : null,
        };
        return config;
    }

    private IConsumer<string, string> CreateConsumer(ConsumerConfig config)
    {
        return UseFakeConsumer
            ? CreateFakeConsumerFunc(config)
            : new ConsumerBuilder<string, string>(config)
                .SetLogHandler((_, logMessage) => { _logger.Debug($"<Consumer>: {logMessage}"); })
                .SetErrorHandler((_, error) => { _logger.Error($"<Consumer>: {error}"); })
                .Build();
    }

    private static void ExecuteInTryCatchWithRethrow(Action func, Action<Exception> onException)
    {
        ExecuteInTryCatchWithRethrow(() =>
        {
            func();
            return true;
        }, onException);
    }

    private static TRes ExecuteInTryCatchWithRethrow<TRes>(Func<TRes> func, Action<Exception> onException)
    {
        try
        {
            return func();
        }
        catch (Exception e)
        {
            onException(e);
            throw;
        }
    }

    protected enum OffsetsCommitStrategy
    {
        OnceAfterAllMessagesProcessed = 0,
        AfterMessageProcessed = 1,
        PeriodicallyMessagedRead = 2,
    }
}
