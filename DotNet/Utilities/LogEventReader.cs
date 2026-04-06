using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LogEventReaderProject
{
    public class LogEventReader<TEvent> where TEvent : class
    {
        private static string LoggerName => $"{nameof(LogEventReader<TEvent>)}<{typeof(TEvent).Name}>";

        private int _lastLatestFileNumber = -1;
        private long _lastReadLength = 0;
        private string _lastUnhandledTextPiece = "";
        private readonly Queue<TEvent> _logEventsQueue = new();

        private readonly string _logFile;
        private readonly ParseEventFunc _parseEventFunc;

        public delegate TEvent ParseEventFunc(string logger, string message, DateTime time);

        public LogEventReader(
            string logFile,
            ParseEventFunc parseEventFunc)
        {
            _logFile = logFile;
            _parseEventFunc = parseEventFunc;
        }

        public void ReadNew()
        {
            // These logs are expected to be empty on chronos/web start. Otherwise entries may be lost on log rotate right after start.
            string logFilesDir = Path.GetDirectoryName(_logFile)!;
            string logFilesNameBase = Path.GetFileName(_logFile);
            GetEventsFromLogs(logFilesDir, logFilesNameBase, ref _lastReadLength, ref _lastUnhandledTextPiece);
        }

        public bool TryDequeue(out TEvent logEvent)
        {
            bool res = _logEventsQueue.TryDequeue(out TEvent ev);
            logEvent = ev;
            return res;
        }

        private void GetEventsFromLogs(
            string logFilesDir, string logFilesNameBase,
            ref long lastReadLength, ref string lastUnhandledTextPiece)
        {
            (string? latestLogFilePath, string? previousLogFilePath) = GetLatestAndPreviousLogFilesPaths();
            if (latestLogFilePath is null)
                return;

            try
            {
                long latestFileSize = new FileInfo(latestLogFilePath).Length;
                if (latestFileSize < lastReadLength)
                {
                    if (previousLogFilePath is null)
                        throw new Exception($"File '{latestLogFilePath}' has just been rotated, but '{previousLogFilePath}' doesn't exist.");
                    Loggers.Monitoring.Debug($"{LoggerName}: File '{latestLogFilePath}' has just been rotated.");

                    long previousFileSize = new FileInfo(previousLogFilePath).Length;
                    if (previousFileSize > lastReadLength)
                    {
                        string text = ReadLastText(previousLogFilePath, ref lastReadLength);
                        Loggers.Monitoring.Trace($"{LoggerName}: Previous file text read:\n{text}");
                        OnTextRead(text, ref lastUnhandledTextPiece);
                    }
                    lastReadLength = 0;
                }

                if (latestFileSize > lastReadLength)
                {
                    string text = ReadLastText(latestLogFilePath, ref lastReadLength);
                    Loggers.Monitoring.Trace($"{LoggerName}: Latest file text read:\n{text}");
                    OnTextRead(text, ref lastUnhandledTextPiece);
                }
            }
            catch (Exception ex)
            {
                Loggers.Monitoring.LogException($"{LoggerName}: Error monitoring file:", ex);
            }

            (string? latestLogFilePath, string? previousLogFilePath) GetLatestAndPreviousLogFilesPaths()
            {
                if (!Directory.Exists(logFilesDir))
                    return (null, null);

                HashSet<string> logFilesNames = [.. Directory.GetFiles(
                    logFilesDir, $"{logFilesNameBase}*.txt", SearchOption.TopDirectoryOnly)
                        .Select(filePath => Path.GetFileName(filePath)),];

                int i = _lastLatestFileNumber == -1 ? 9999 : _lastLatestFileNumber + 1;
                string? latestLogFileName = null;
                string? previousLogFileName = null;
                while (latestLogFileName is null && i >= 0)
                {
                    string fileName = GetLogFileName(i);
                    if (logFilesNames.Contains(fileName))
                    {
                        latestLogFileName = fileName;
                        _lastLatestFileNumber = i;
                    }
                    i--;
                }
                if (latestLogFileName is not null && logFilesNames.Contains(GetLogFileName(i)))
                    previousLogFileName = GetLogFileName(i);

                string? latestLogFilePath = latestLogFileName is null ? null : Path.Join(logFilesDir, latestLogFileName);
                string? previousLogFilePath = previousLogFileName is null ? null : Path.Join(logFilesDir, previousLogFileName);
                return (latestLogFilePath, previousLogFilePath);

                string GetLogFileName(int i) => i == 0 ? $"{logFilesNameBase}.txt" : $"{logFilesNameBase}_{i:0000}.txt";
            }
        }

        private void GetEventsFromLogs_OldRotationLogic(
            string logFilesDir, string logFilesNameBase,
            ref long lastReadLength, ref string lastUnhandledTextPiece)
        {
            string logFilesPathBase = Path.Join(logFilesDir, logFilesNameBase);
            string mainLogFilePath = $"{logFilesPathBase}.txt";
            string previousLogFilePath = $"{logFilesPathBase}.0.txt";
            if (!File.Exists(mainLogFilePath))
                return;

            try
            {
                long mainFileSize = new FileInfo(mainLogFilePath).Length;
                if (mainFileSize < lastReadLength)
                {
                    Loggers.Monitoring.Debug($"{LoggerName}: Log rotation detected for '{logFilesPathBase}'");
                    long previousFileSize = new FileInfo(previousLogFilePath).Length;
                    if (previousFileSize > lastReadLength)
                    {
                        string text = ReadLastText(previousLogFilePath, ref lastReadLength);
                        Loggers.Monitoring.Trace($"{LoggerName}: Previous file text read:\n{text}");
                        OnTextRead(text, ref lastUnhandledTextPiece);
                    }
                    lastReadLength = 0;
                }

                if (mainFileSize > lastReadLength)
                {
                    string text = ReadLastText(mainLogFilePath, ref lastReadLength);
                    Loggers.Monitoring.Trace($"{LoggerName}: Main file text read:\n{text}");
                    OnTextRead(text, ref lastUnhandledTextPiece);
                }
            }
            catch (Exception ex)
            {
                Loggers.Monitoring.LogException($"{LoggerName}: Error monitoring file:", ex);
            }
        }

        private static string ReadLastText(string filePath, ref long lastReadLength)
        {
            StringBuilder sb = new();
            using FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fs.Seek(lastReadLength, SeekOrigin.Begin);
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead = fs.Read(buffer, 0, buffer.Length);
                lastReadLength += bytesRead;
                if (bytesRead == 0)
                    break;
                string text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                sb.Append(text);
            }
            return sb.ToStringAndRelease();
        }

        private void OnTextRead(string text, ref string lastUnhandledTextPiece)
        {
            if (!string.IsNullOrWhiteSpace(lastUnhandledTextPiece))
            {
                Loggers.Monitoring.Warn($"{LoggerName}: Unhandled text piece:\n{lastUnhandledTextPiece}");
                text = (lastUnhandledTextPiece + text).Trim();
            }

            MatchCollection matches = Regex.Matches(
                text,
                @"\$\$\$\$\$BEGIN\$\$\$\$\$\r?\n(?<time>[\d\s\-\:\.]{24})\r?\n(?<logger>.+?)\r?\n(?<message>.*?)\r?\n\$\$\$\$\$END\$\$\$\$\$",
                RegexOptions.Singleline);

            int lastHandledSymbolIndex = 0;
            if (matches.Count == 0)
                Loggers.Monitoring.Warn($"{LoggerName}: Zero matches for text:\n{text}");
            foreach (Match match in matches)
            {
                string logger = match.Groups["logger"].Value;
                string message = match.Groups["message"].Value;
                string timeStr = match.Groups["time"].Value;
                DateTime time = DateTime.ParseExact(timeStr, "yyyy-MM-dd HH:mm:ss.ffff", null, DateTimeStyles.AssumeLocal);

                TEvent ev = _parseEventFunc(logger, message, time);
                _logEventsQueue.Enqueue(ev);

                lastHandledSymbolIndex = match.Index + match.Length;
            }

            if (text.Length > lastHandledSymbolIndex + 1)
                lastUnhandledTextPiece = text.Substring(lastHandledSymbolIndex + 1);
            else
                lastUnhandledTextPiece = "";
        }
    }
}
