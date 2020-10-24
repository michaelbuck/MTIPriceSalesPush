using System;
using System.Data.SqlClient;


namespace MTIPriceSalesPush
{
  public class Event : IIEvent
  {
    public string AtgType;

   

    #region Declarations

    public int CompanyId;
    public int SiteId;
    public ICom Port;
    public int CommDeviceId;

    public string EventName;
    public int EventId;
    public bool Success;
    public string ConnectionString;

    public int ScheduleDuration;
    public TimeSpan Duration;
    public DateTimeOffset StartTime;
    public int BytesSent;
    public int BytesRcvd;
    public bool CallOpenPort;
    public string EventHandler;
    public string ThreadId;
    public string TimeZoneSystemId;
    public TimeZoneInfo SiteTimeZoneInfo;

    public INLogger SysLog;
    protected int LogLevel;

    #endregion

    protected Event(ICom iport = null)
    {
      Port = iport ?? new OCom();

      Duration = TimeSpan.Zero;
      BytesRcvd = 0;
      BytesSent = 0;
      Success = true;
    }

    public virtual void OpenPort()
    {
      StartEvent();
      if (CallOpenPort)
        Port.OpenPort();
    }

    public virtual void ClosePort()
    {
      EndEvent();
      Port.ClosePort();
    }

    public int EventLogLevel
    {
      get { return LogLevel; }
      set
      {
        Port.LogLevel = value;
        LogLevel = value;
      }
    }


    public void StartEvent()
    {
      StartTime = DateTime.Now;
    }

    public void EndEvent()
    {
      Duration = DateTime.Now - StartTime;
    }

    public virtual void RunEvent()
    {

    }

 
  }
}
