using Azure.Storage.Queues;
using System.Text.Encodings;
using System;
using System.Text;
using Chilkat;
using Azure.Storage.Queues.Models;
using IniParser;
using IniParser.Model;
using System.IO;
using System.Runtime.Serialization;

namespace MTIPriceSalesPush
{
  class Program
  {
    static NLogger Syslog;
    public static int posport;
    public static string posIPAddress;
    public static string posUserName;
    public static string posPassword;
    public static string posAuthKey;

    static void Main()
    {
      Syslog = new NLogger("Main");
      Syslog.LogTrace("{0}", "Service Startup..");

      if (!File.Exists("MTI.ini"))
      {
        InitializeMTIiNi();
      }
      var iniFile = new FileIniDataParser();
      IniData inidata = iniFile.ReadFile("MTI.ini");
      posport = Convert.ToInt32(inidata["POS"]["Port"]);
      posIPAddress = inidata["POS"]["IPAddr"];
      posUserName = inidata["POS"]["UserName"];
      posPassword = inidata["POS"]["Password"];
      /*
      var interval = Properties.Settings.Default.UpdateTimer*10000;  // Minutes
      var runtimer = new System.Timers.Timer(interval);
      runtimer.Elapsed += Runtimer_Elapsed;
      runtimer.AutoReset = true;
      runtimer.Enabled = true;
      Syslog.LogInfo("{0}","Timer started....");
      */
      Run();

      Console.ReadKey();
    }

    private static void InitializeMTIiNi()
    {
      var iniFile = new FileIniDataParser();
      iniFile.WriteFile("MTI.ini", new IniData());
      IniData data = iniFile.ReadFile("MTI.ini");
      data.Sections.AddSection("SiteInfo");
      data["SiteInfo"].AddKey("SiteId", "0");
      data["SiteInfo"].AddKey("CompanyId", "0");
      data["SiteInfo"].AddKey("UpdateMin", "15");
      data.Sections.AddSection("POS");
      data["POS"].AddKey("IPAddr", "192.168.31.11");
      data["POS"].AddKey("Port", "443");
      data["POS"].AddKey("UserName", "mytankinfo");
      data["POS"].AddKey("Password", "a000000");
      data["POS"].AddKey("IPAddr", "192.168.31.11");
      data["POS"].AddKey("AuthKey", "0");
      data.Sections.AddSection("Prices");
      iniFile.WriteFile("MTI.ini", data);
    }

    private static void Run()
    {

      var result = Connect_GetFuelConfig();
      if (result == "NewAuthRequired")
      {
        Connect_GetPosAuthKey();
        Connect_GetFuelConfig();
        Connect_GetPosTotalizers();
      }
      else
      {
        Connect_GetPosTotalizers();
      }
    }
    
    private static string Connect_GetPosTotalizers()
    {
      comVerifonePOS com;
      var port = new OCom
      {
        ComType = OCom.CommsType.Socket,
        Port = posport,
        HostName = posIPAddress
      };
      try
      {
        port.OpenSslPort();
        com = new comVerifonePOS(port)
        {
          posUserName = posUserName,
          posPassword = posPassword
        };
        com.GetPosTotalizers();
        com.ClosePort();
      }
      catch (Exception ex)
      {
        Syslog.LogError(ex.Message);

        return ex.Message;
      }
      com.ClosePort();
      return "Success";
    }

    private static string Connect_GetPosAuthKey()
    {
      comVerifonePOS com;
      var port = new OCom
      {
        ComType = OCom.CommsType.Socket,
        Port = posport,
        HostName = posIPAddress
      };
      try
      {
        port.OpenSslPort();
        com = new comVerifonePOS(port)
        {
          posUserName = posUserName,
          posPassword = posPassword
        };
        com.GetPosAuthKey();
        com.ClosePort();
      }
      catch (Exception ex)
      {
        Syslog.LogError(ex.Message);
        return ex.Message;
      }
      com.ClosePort();
      return "Success";
    }

    private static string Connect_GetFuelConfig()
    {
      string result;
      var port = new OCom
      {
        ComType = OCom.CommsType.Socket,
        Port = posport,
        HostName = posIPAddress
      };
      try
      {
        port.OpenSslPort();
        comVerifonePOS com = new comVerifonePOS(port);
        com.posUserName = posUserName;
        com.posPassword = posPassword;
        
        result = com.GetFuelConfig();
        com.ClosePort();
        return result;
      }
      catch (Exception ex)
      {
        Syslog.LogError(ex.Message);
        return ex.Message;
      }
    }

    private static void Runtimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      Syslog.LogInfo("{0}", "Timer Fired....");
      var result = Connect_GetFuelConfig();
      if (result == "NewAuthRequired")
      {
        Connect_GetPosAuthKey();
        Connect_GetFuelConfig();
        Connect_GetPosTotalizers();
      }
      else
      {
        Connect_GetPosTotalizers();
      }
    }
  }
}
