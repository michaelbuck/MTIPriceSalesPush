using System;
using System.Configuration;
using NLog;

namespace MTIPriceSalesPush
{
  public class NLogger : INLogger
  {
    public int MinLogLevel { get; set; }
    //public string IPAddress { get; set; }
    //public string CompanyId { get; set; }
    //public string SiteId { get; set; }

  
    private readonly Logger _logger;


    //---------------------------------------------------------------------
    public NLogger(string name)
    {
      _logger = LogManager.GetLogger(name);
      //IPAddress = Properties.Settings.Default.PosIPAddress;
      //CompanyId = Properties.Settings.Default.MTICompanyId;
      //SiteId = Properties.Settings.Default.MTISiteId;
      // Default MinLogLevel to 5
      MinLogLevel = 0;
    }

    /// <summary>
    /// Creates an instance of a SQLogger that uses the name of the given Type.
    /// </summary>
    /// <param name="classType">Type of the class that is using this SQLogger.</param>
    public NLogger(Type classType) 
      :this(classType.Name)
    {
    }

    //---------------------------------------------------------------------
    public void Write2Log(string message, LogLevel level, int logLevel)
    {
      if (logLevel < MinLogLevel) return;

      var logEvent = new LogEventInfo(level, _logger.Name, message);
      // set event-specific context parameter
      logEvent.Properties["level"] = logLevel;
      //logEvent.Properties["companyid"] = CompanyId;
      //logEvent.Properties["siteid"] = SiteId;
      //logEvent.Properties["logger"] = _logger.Name;
      //logEvent.Properties["posipaddress"] = IPAddress;

      // Call the Log() method. It is important to pass typeof(SQLogger) as the
      // first parameter. If you don't, ${callsite} and other callstack-related 
      // layout renderers will not work properly.
      //
      _logger.Log(typeof (NLogger), logEvent);
    }

    /// <summary>
    /// Logs a message with a log level of Trace.
    /// </summary>
    /// <param name="eventId">ID for the event type</param>
    /// <param name="format">A to log with optional formatting place holders</param>
    /// <param name="args">Optional array of objects to merge with the format string</param>
    public void LogTrace(string format, params object[] args)
    {
      var message = string.Format(format, args);
      Write2Log(message, LogLevel.Trace, 0);
    }

    /// <summary>
    /// Logs a message with a log level of Debug.
    /// </summary>
    /// <param name="format">A to log with optional formatting place holders</param>
    /// <param name="args">Optional array of objects to merge with the format string</param>
    public void LogDebug(string format, params object[] args)
    {
      var message = string.Format(format, args);
      Write2Log(message, LogLevel.Debug, 5);
    }

    /// <summary>
    /// Logs a message with a log level of Info.
    /// </summary>
    /// <param name="format">A to log with optional formatting place holders</param>
    /// <param name="args">Optional array of objects to merge with the format string</param>
    public void LogInfo(string format, params object[] args)
    {
      var message = string.Format(format, args);
      Write2Log(message, LogLevel.Info, 7);
    }

    /// <summary>
    /// Logs a message with a log level of Error.
    /// </summary>
    /// <param name="format">A to log with optional formatting place holders</param>
    /// <param name="args">Optional array of objects to merge with the format string</param>
    public void LogError(string format, params object[] args)
    {
      var message = string.Format(format, args);
      Write2Log(message, LogLevel.Error, 10);
    }

    /// <summary>
    /// Logs the Message of the given Exception with a log level of Error.
    /// </summary>
    /// <param name="exception">Exception to log</param>
    public void LogError( Exception exception)
    {
      Write2Log(exception.Message, LogLevel.Error, 10);
    }
  }
}