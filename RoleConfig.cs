using System;
using Microsoft.WindowsAzure;
using Microsoft.Azure;
using System.Configuration;

namespace MTIPriceSalesPush.Shared
{
  public static class RoleConfig
  {
    /// <summary>
    ///   Returns the storage connection string from the role configuration.
    /// </summary>
    public static string StorageConnectionString => ConfigurationManager.AppSettings["StorageConnectionString"];

    /// <summary>
    ///   Returns the database connection string from the role configuration.
    /// </summary>
    //public static string DatabaseConnectionString => ConfigurationManager.AppSettings["ConnectionString"];

    public static int GetMinLogLevel(string prefix)
    {
      return GetRoleInt(prefix, "MinLogLevel", 5);
    }

    public static int GetQueueMessagesPerRequest(string prefix)
    {
      return GetRoleInt(prefix, "QMsgsPerRequest", 5);
    }

    public static TimeSpan GetQueueCheckTimeIncrement(string prefix)
    {
      return GetRoleTimeSpan(prefix, "QIncrementSec", 30);
    }

    public static int GetQueueMaximumTimeIncrements(string prefix)
    {
      return GetRoleInt(prefix, "QMaxIncrements", 5);
    }

    /// <summary>
    ///   Retrieves an integer value from the role configuration using a prefix and
    ///   a generic setting name (i.e., "RulesEngine" + "MaxTime" = "RulesEngineMaxTime")
    ///   or returns the given default value if it fails to do so.
    /// </summary>
    /// <param name="prefix">A string to affix to the beginning of the given setting name.</param>
    /// <param name="settingName">Generic name of the configuration setting to retrieve.</param>
    /// <param name="defaultValue">Value to return if this method fails to get the setting.</param>
    /// <returns>int</returns>
    public static int GetRoleInt(string prefix, string settingName, int defaultValue = 0)
    {
      var key = string.Format("{0}{1}", prefix, settingName);
      return GetRoleInt(key, defaultValue);
    }

    /// <summary>
    ///   Retrieves an integer value from the role configuration,
    ///   or returns the given default value if it fails to do so.
    /// </summary>
    /// <param name="key">Name of the configuration setting to retrieve.</param>
    /// <param name="defaultValue">Value to return if this method fails to get the setting.</param>
    /// <returns>int</returns>
    public static int GetRoleInt(string key, int defaultValue = 0)
    {
      try
      {
        var value = ConfigurationManager.AppSettings[key];
        int result;
        return (int.TryParse(value, out result)) ? result : defaultValue;
      }
      catch
      {
      }
      return defaultValue;
    }

    /// <summary>
    ///   Retrieves a TimeSpan from the role configuration using a prefix and
    ///   a generic setting name (i.e., "RulesEngine" + "MaxTime" = "RulesEngineMaxTime")
    ///   or returns a TimeSpan with the given number of seconds if it fails to do so.
    /// </summary>
    /// <param name="prefix">A string to affix to the beginning of the given setting name.</param>
    /// <param name="settingName">Generic name of the configuration setting to retrieve.</param>
    /// <param name="defaultSeconds">Number of seconds to use for the TimeSpan if this method fails to get the setting.</param>
    /// <returns>TimeSpan</returns>
    public static TimeSpan GetRoleTimeSpan(string prefix, string settingName, int defaultSeconds = 60)
    {
      var key = string.Format("{0}{1}", prefix, settingName);
      return GetRoleTimeSpan(key, defaultSeconds);
    }

    /// <summary>
    ///   Retrieves a TimeSpan from the role configuration,
    ///   or returns a TimeSpan with the given number of seconds if it fails to do so.
    /// </summary>
    /// <param name="key">Name of the configuration setting to retrieve. It is assumed the setting is in seconds.</param>
    /// <param name="defaultSeconds">Number of seconds to use for the TimeSpan if this method fails to get the setting.</param>
    /// <returns>TimeSpan</returns>
    private static TimeSpan GetRoleTimeSpan(string key, int defaultSeconds = 60)
    {
      try
      {
        var seconds = GetRoleInt(key, defaultSeconds);
        return TimeSpan.FromSeconds(seconds);
      }
      catch
      {
      }
      return TimeSpan.FromSeconds(defaultSeconds);
    }
  }
}