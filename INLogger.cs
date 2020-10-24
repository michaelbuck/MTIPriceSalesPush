using System;
using NLog;

namespace MTIPriceSalesPush
{
  public interface INLogger
  {
    int MinLogLevel { get; set; }
    //string IPAddress { get; set; }


    void Write2Log(string message, LogLevel level, int logLevel);

    /// <summary>
    ///   Logs a message with a log level of Trace.
    /// </summary>
    /// <param name="format">A to log with optional formatting place holders</param>
    /// <param name="args">Optional array of objects to merge with the format string</param>
    void LogTrace(string format, params object[] args);

    /// <summary>
    ///   Logs a message with a log level of Debug.
    /// </summary>
    /// <param name="format">A to log with optional formatting place holders</param>
    /// <param name="args">Optional array of objects to merge with the format string</param>
    void LogDebug(string format, params object[] args);

    /// <summary>
    ///   Logs a message with a log level of Info.
    /// </summary>
    /// <param name="format">A to log with optional formatting place holders</param>
    /// <param name="args">Optional array of objects to merge with the format string</param>
    void LogInfo(string format, params object[] args);

    /// <summary>
    ///   Logs a message with a log level of Error.
    /// </summary>
    /// <param name="format">A to log with optional formatting place holders</param>
    /// <param name="args">Optional array of objects to merge with the format string</param>
    void LogError(string format, params object[] args);

    /// <summary>
    ///   Logs the Message of the given Exception with a log level of Error.
    /// </summary>
    /// <param name="exception">Exception to log</param>
    void LogError(Exception exception);
  }
}