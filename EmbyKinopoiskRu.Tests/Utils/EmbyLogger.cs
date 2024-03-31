using System.Globalization;
using System.Text;

using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.Tests.Utils;

public class EmbyLogger : ILogger
{
    private readonly NLog.Logger _logger;

    public EmbyLogger(NLog.Logger logger)
    {
        _logger = logger;
    }

    public void Debug(ReadOnlyMemory<char> message)
    {
        _logger.Debug(message.ToString());
    }

    public void Debug(string message, params object[] paramList)
    {
        _logger.Debug(CultureInfo.InvariantCulture, message, paramList);
    }

    public void Error(ReadOnlyMemory<char> message)
    {
        _logger.Error(message.ToString());
    }

    public void Error(string message, params object[] paramList)
    {
        _logger.Error(CultureInfo.InvariantCulture, message, paramList);
    }

    public void ErrorException(string message, Exception exception, params object[] paramList)
    {
        _logger.Error(exception, message, paramList);
    }

    public void Fatal(string message, params object[] paramList)
    {
        _logger.Fatal(CultureInfo.InvariantCulture, message, paramList);
    }

    public void FatalException(string message, Exception exception, params object[] paramList)
    {
        _logger.Fatal(exception, message, paramList);
    }

    public void Info(ReadOnlyMemory<char> message)
    {
        _logger.Info(message.ToString());
    }

    public void Info(string message, params object[] paramList)
    {
        _logger.Info(CultureInfo.InvariantCulture, message, paramList);
    }

    public void Log(LogSeverity severity, string message, params object[] paramList)
    {
        switch (severity)
        {
            case LogSeverity.Debug:
                Debug(message, paramList);
                break;
            case LogSeverity.Info:
                Info(message, paramList);
                break;
            case LogSeverity.Warn:
                Warn(message, paramList);
                break;
            case LogSeverity.Error:
                Error(message, paramList);
                break;
            case LogSeverity.Fatal:
                Fatal(message, paramList);
                break;
        }
    }

    public void Log(LogSeverity severity, ReadOnlyMemory<char> message)
    {
        switch (severity)
        {
            case LogSeverity.Debug:
                Debug(message);
                break;
            case LogSeverity.Info:
                Info(message);
                break;
            case LogSeverity.Warn:
                Warn(message);
                break;
            case LogSeverity.Error:
                Error(message);
                break;
            case LogSeverity.Fatal:
                Fatal(message.ToString());
                break;
        }
    }

    public void LogMultiline(string message, LogSeverity severity, StringBuilder additionalContent)
    {
        if (additionalContent == null)
        {
            throw new ArgumentNullException(nameof(additionalContent));
        }

        Log(severity, additionalContent.Insert(0, message + "\n").ToString());
    }

    public void Warn(ReadOnlyMemory<char> message)
    {
        _logger.Warn(message.ToString());
    }

    public void Warn(string message, params object[] paramList)
    {
        _logger.Warn(CultureInfo.InvariantCulture, message, paramList);
    }
}
