using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Chilkat;
using MTIPriceSalesPush.Verifone;
using MTIPriceSalesPush.Verifone.Commands;
using IniParser;
using IniParser.Model;
using StringBuilder = System.Text.StringBuilder;
using Newtonsoft.Json;

namespace MTIPriceSalesPush
{
  #region Classes
  public class Totalizer
  {
    public int Position { get; set; }
    public string Product { get; set; }
    public string UofM { get; set; }
    public float Volume { get; set; }
    public string Currency { get; set; }
    public float Money { get; set; }
    public int ProductId { get; set; }
  }

  public class Blend
  {
    public int TankId { get; set; }
    public int PosProductId { get; set; }
    public int AtgProductId { get; set; }
    public int BlendPercent { get; set; }
  }

  public class TankId2ProdId
  {
    public int TankId { get; set; }
    public int ProductId { get; set; }
    public string TankName { get; set; }
  }

  public class Price
  {
    public int ServiceLevelId { get; set; }
    public int MopLevelId { get; set; }
    public int Tier { get; set; }
    public decimal PriceLevel { get; set; }
  }

  public class PriceChange
  {
    public string ProductName { get; set; }
    public string MOPName { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal LastPrice { get; set; }
  }
  public class PriceChangeMessage
  {
    public int SiteId { get; set; }
    public List<PriceChange> PriceChanges = new List<PriceChange>();
  }
  // ReSharper disable once InconsistentNaming
  public class MOP
  {
    public string MopName { get; set; }
    public int MopId { get; set; }
  }

  public class ServiceLevel
  {
    public string ServiceLevelName { get; set; }
    public int ServiceLevelId { get; set; }
  }
  public class ProductPrice
  {
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public List<MOP> Mops { get; set; }
    public List<Blend> Blends { get; set; }
    public List<Price> Prices { get; set; }
    public List<ServiceLevel> ServiceLevels { get; set; }
    public List<TankId2ProdId> Tanks { get; set; }
  }
  #endregion

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
  public class comVerifonePOS : Event
  {
    private string _posUrlPattern = "/cgi-bin/CGILink?";
    public CommandArgs commandArgs;
    private string _commandCookie;

    private IEnumerable<ProductPrice> _productprice;
    private IEnumerable<MOP> _mops;
    private IEnumerable<ServiceLevel> _servicelvls;
    private IEnumerable<Totalizer> _totalizers;
    private List<PosTotal> _postotals;

    public string posUserName;
    public string posPassword;
    public string posAuthKey;
    public string timezone;
    public IniData inidata;
    public FileIniDataParser inifile;

    public comVerifonePOS(ICom iport = null, INLogger logger = null) : base(iport)
    {
      AtgType = "Verifone POS";
      SysLog = logger ?? new NLogger("comVerifonePOS");
      inifile = new FileIniDataParser();
      inidata = inifile.ReadFile("MTI.ini");
      SiteId = Convert.ToInt32(inidata["SiteInfo"]["SiteId"]);
    }
    public override void RunEvent()
    {
      Port.EventId = EventId;
      Port.LogLevel = EventLogLevel;
      SysLog.MinLogLevel = EventLogLevel;
      base.RunEvent();
    }

    // Add routines that will open the TCP connection to the POS
    public string GetFuelConfig()
    {
      try
      {
        InitializeCommandArgs();
        var getFuelConfigCommand = new RequestFuelConfigCommand(commandArgs);
        var getFuelConfigResponse = getFuelConfigCommand.Execute();
        if (getFuelConfigResponse != null)
        {
          var response = new HtmlToXml();
          response.Html = getFuelConfigResponse.ToString();
          var xdoc = XDocument.Parse(response.ToXml());
          var node = xdoc.Descendants().FirstOrDefault(n => (n.Name == "faultcode" || n.Name == "faultCode"));
          if (node != null)
          {
            var errstr = new StringBuilder();
            errstr.Append("faultcode:");
            errstr.Append(node.Value);
            var nextnode = xdoc.Descendants().FirstOrDefault(n => (n.Name == "faultstring" || n.Name == "faultString"));
            if (nextnode != null)
            {
              errstr.Append("| faultstring:");
              errstr.Append(nextnode.Value);
            }
            nextnode = xdoc.Descendants().FirstOrDefault(n => n.Name == "detail");
            if (nextnode != null)
            {
              errstr.Append("| detail:");
              errstr.Append(nextnode.Value);
              SysLog.LogError("Unable to get FuelConfig...{0}", errstr.ToString());
              return "Error";
            }
            return "Success";
          }
          else
          {
            ParseFuelCfg(getFuelConfigResponse.ToString());
            return "Success";
          }
        }
        else
        {
          SysLog.Write2Log("Calling GetPosAuthKey..", NLog.LogLevel.Debug, 10);
          return "NewAuthRequired";
        }
      }
      catch (Exception ex)
      {
        SysLog.Write2Log(ex.Message, NLog.LogLevel.Error, 10);
        return "Error";
      }
    }

    public void GetPosTotalizers()
    {
      try
      {
        InitializeCommandArgs();
        var getTotalizersCommand = new RequestTotalizersCommand(commandArgs);
        var getTotalizersResponse = getTotalizersCommand.Execute();
        if (getTotalizersResponse != null)
        {
          var response = new HtmlToXml
          {
            Html = getTotalizersResponse.ToString()
          };
          var xdoc = XDocument.Parse(response.ToXml());
          var node = xdoc.Descendants().FirstOrDefault(n => (n.Name == "faultcode" || n.Name == "faultCode"));
          if (node != null)
          {
            var errstr = new StringBuilder();
            errstr.Append("faultcode:");
            errstr.Append(node.Value);
            var nextnode = xdoc.Descendants().FirstOrDefault(n => (n.Name == "faultstring" || n.Name == "faultString"));
            if (nextnode != null)
            {
              errstr.Append("| faultstring:");
              errstr.Append(nextnode.Value);
            }
            nextnode = xdoc.Descendants().FirstOrDefault(n => n.Name == "detail");
            if (nextnode != null)
            {
              errstr.Append("| detail:");
              errstr.Append(nextnode.Value);
              SysLog.LogError("Unable to get POSTotalizers...{0}", errstr.ToString());
            }
          }
          else
          {
            ParseRunningHoseTotal(getTotalizersResponse.ToString());
          }
        }
        else
        {
          //          SysLog.Write2Log("Enabling GetPosAuthKey Event..", NLog.LogLevel.Debug, 10);
          //          EnableGetPosAuthKeyEvent();
          //          SysLog.Write2Log("Re-enabling GetPosTotalizers Event..", NLog.LogLevel.Debug, 10);          
        }
      }
      catch (Exception ex)
      {
        SysLog.Write2Log(ex.Message, NLog.LogLevel.Error, 10);
      }
    }

    public string GetSummary()
    {
      try
      {
        InitializeCommandArgs();
        var getSummaryCommand = new RequestSummaryCommand(commandArgs);
        var getSummaryResponse = getSummaryCommand.Execute();
        if (getSummaryResponse != null)
        {
          var response = new HtmlToXml();
          var success = response.UnlockComponent("MYTANK.CB1022021_GSgqBcCh6Hj3 ");
          response.Html = getSummaryResponse.ToString();
          var xdoc = XDocument.Parse(response.ToXml());
          var node = xdoc.Descendants().FirstOrDefault(n => (n.Name == "faultcode" || n.Name == "faultCode"));
          if (node != null)
          {
            return "Error";
          }
          else
          {
            ParseSummary(getSummaryResponse.ToString());
            return "Success";
          }
          #region temp
          /*          var node = xdoc.Descendants().FirstOrDefault(n => (n.Name == "faultcode" || n.Name == "faultCode"));
                    if (node != null)
                    {
                      var errstr = new StringBuilder();
                      errstr.Append("faultcode:");
                      errstr.Append(node.Value);
                      var nextnode = xdoc.Descendants().FirstOrDefault(n => (n.Name == "faultstring" || n.Name == "faultString"));
                      if (nextnode != null)
                      {
                        errstr.Append("| faultstring:");
                        errstr.Append(nextnode.Value);
                        return "Error";
                      }
                      nextnode = xdoc.Descendants().FirstOrDefault(n => n.Name == "detail");
                      if (nextnode != null)
                      {
                        errstr.Append("| detail:");
                        errstr.Append(nextnode.Value);
                        return "Error";
                        //SysLog.LogError(EventId, "Unable to get Summary...{0}", errstr.ToString());
                      }
                      else
                      {
                        ParseSummary(getSummaryResponse.ToString());
                        return "Success";
                      }

                    }
                    else
                    {
                      ParseSummary(getSummaryResponse.ToString());
                      return "Success";
                    }
                  }
                  else
                  {
                    SysLog.Write2Log("Calling GetPosAuthKey..", NLog.LogLevel.Debug, 10);
                    return "NewAuthRequired";
                    //SysLog.Write2Log(EventId, "Enabling GetPosAuthKey Event..", NLog.LogLevel.Debug, 10);
                    //EnableGetPosAuthKeyEvent();
                    //SysLog.Write2Log(EventId, "Re-enabling GetPosTotalizers Event..", NLog.LogLevel.Debug, 10);
                    //EnableGetPosTotalizers();
                  }*/
          #endregion
        }
        return "Error";
      }
      catch (Exception ex)
      {
        SysLog.Write2Log(ex.Message, NLog.LogLevel.Error, 10);
        return "Error";
      }
    }


    public string GetHoseTest()
    {
      try
      {
        InitializeCommandArgs();
        var getHoseTestCommand = new RequestHoseTestCommand(commandArgs);
        var getHoseTestResponse = getHoseTestCommand.Execute();
        if (getHoseTestResponse != null)
        {
          var response = new HtmlToXml();
          var success = response.UnlockComponent("MYTANK.CB1022021_GSgqBcCh6Hj3 ");
          response.Html = getHoseTestResponse.ToString();
          var xdoc = XDocument.Parse(response.ToXml());
          var node = xdoc.Descendants().FirstOrDefault(n => (n.Name == "faultcode" || n.Name == "faultCode"));
          if (node != null)
          {
            return "Error";
          }
          else
          {
            ParseHoseTest(getHoseTestResponse.ToString());
            return "Success";
          }
          #region temp
          /*          var node = xdoc.Descendants().FirstOrDefault(n => (n.Name == "faultcode" || n.Name == "faultCode"));
                    if (node != null)
                    {
                      var errstr = new StringBuilder();
                      errstr.Append("faultcode:");
                      errstr.Append(node.Value);
                      var nextnode = xdoc.Descendants().FirstOrDefault(n => (n.Name == "faultstring" || n.Name == "faultString"));
                      if (nextnode != null)
                      {
                        errstr.Append("| faultstring:");
                        errstr.Append(nextnode.Value);
                        return "Error";
                      }
                      nextnode = xdoc.Descendants().FirstOrDefault(n => n.Name == "detail");
                      if (nextnode != null)
                      {
                        errstr.Append("| detail:");
                        errstr.Append(nextnode.Value);
                        return "Error";
                        //SysLog.LogError(EventId, "Unable to get Summary...{0}", errstr.ToString());
                      }
                      else
                      {
                        ParseSummary(getSummaryResponse.ToString());
                        return "Success";
                      }

                    }
                    else
                    {
                      ParseSummary(getSummaryResponse.ToString());
                      return "Success";
                    }
                  }
                  else
                  {
                    SysLog.Write2Log("Calling GetPosAuthKey..", NLog.LogLevel.Debug, 10);
                    return "NewAuthRequired";
                    //SysLog.Write2Log(EventId, "Enabling GetPosAuthKey Event..", NLog.LogLevel.Debug, 10);
                    //EnableGetPosAuthKeyEvent();
                    //SysLog.Write2Log(EventId, "Re-enabling GetPosTotalizers Event..", NLog.LogLevel.Debug, 10);
                    //EnableGetPosTotalizers();
                  }*/
          #endregion
        }
        return "Error";
      }
      catch (Exception ex)
      {
        SysLog.Write2Log(ex.Message, NLog.LogLevel.Error, 10);
        return "Error";
      }
    }


    public void UpdatePosPassword()
    {
      try
      {
        _posUrlPattern = "/cgi-bin/CGIUplink?";
        InitializeCommandArgs();
        var updatePwdRequest = new UpdatePasswordCommand(commandArgs);
        var getUpdatePwdResponse = updatePwdRequest.ExecutePost();
        // must ensure it was successful before I save it
        if (getUpdatePwdResponse != null)
        {
          var response = new HtmlToXml();
          response.Html = getUpdatePwdResponse.ToString();
          var xdoc = XDocument.Parse(response.ToXml());
          var node = xdoc.Descendants().FirstOrDefault(n => (n.Name == "faultcode" || n.Name == "faultCode"));
          if (node != null)
          {
            var errstr = new StringBuilder();
            errstr.Append("faultcode:");
            errstr.Append(node.Value);
            var nextnode = xdoc.Descendants().FirstOrDefault(n => (n.Name == "faultstring" || n.Name == "faultString"));
            if (nextnode != null)
            {
              errstr.Append("| faultstring:");
              errstr.Append(nextnode.Value);
            }
            nextnode = xdoc.Descendants().FirstOrDefault(n => n.Name == "detail");
            if (nextnode != null)
            {
              errstr.Append("| detail:");
              errstr.Append(nextnode.Value);
            }
            SysLog.LogError("Password NOT Changed..{0}", errstr.ToString());
          }
          else
          {
            SavePosPassword(updatePwdRequest.Newpassword);
            SysLog.LogInfo("Password Change to {0} Sucessful!..", updatePwdRequest.Newpassword);
          }
        }
        else
        {
          //          SysLog.Write2Log("Enabling GetPosAuthKey Event..", NLog.LogLevel.Debug, 10);
          //          EnableGetPosAuthKeyEvent();
          //          SysLog.Write2Log("Re-enabling UpdatePOSPassword Event..", NLog.LogLevel.Debug, 10);
          //          EnableUpdatePosPassword();
        }
      }
      catch (Exception ex)
      {
        SysLog.Write2Log(ex.Message, NLog.LogLevel.Error, 10);
      }
    }
    /*
        private void EnableUpdatePosPassword()
        {
          UnitOfWork.ExecuteSql("UPDATE Events SET NextRunAt='1-1-2012' WHERE Id=@EventId",
            new SqlParameter("@EventId", EventId));
        }
    */
    public void GetPosAuthKey()
    {
      try
      {
        InitializeCommandArgs();
        var getCookieRequest = new RequestCookieCommand(commandArgs);
        var getCookieResponse = getCookieRequest.Execute();
        if (getCookieResponse != null)
        {
          _commandCookie = getCookieRequest.Cookie;
          SavePosAuthKey(_commandCookie);
          SysLog.LogTrace("PosAuthKey={0}", _commandCookie);
        }
        else
        {
          SysLog.LogError("UserName or Password incorrect... No authority");
        }
      }
      catch (Exception ex)
      {
        SysLog.Write2Log(ex.Message, NLog.LogLevel.Error, 10);
      }
    }

    private void ReleasePosCookie()
    {
      try
      {
      }
      catch (Exception ex)
      {
        SysLog.Write2Log(ex.Message, NLog.LogLevel.Error, 10);
      }
    }

    private void InitializeCommandArgs()
    {
      try
      {
        commandArgs = new CommandArgs
        {
          User = posUserName,
          Password = posPassword,
          Socket = Port.Socket,
          Cookie = posAuthKey,
          Url = _posUrlPattern,
          sysLog = SysLog,
          eventid = EventId
        };
      }
      catch (Exception ex)
      {
        SysLog.Write2Log(ex.Message, NLog.LogLevel.Error, 10);
      }
    }

    private void SavePosAuthKey(string cookie)
    {
      posAuthKey = cookie;
      inidata["POS"]["AuthKey"] = cookie;
      inifile.WriteFile("MTI.ini", inidata);
    }

    private void SavePosPassword(string password)
    {
      inidata["POS"]["Password"] = password;
      inifile.WriteFile("MTI.ini", inidata);
    }
    public void ParseFuelCfg(string datastream)
    {
      var fuelprices = XDocument.Parse(datastream);
      var mytanks = CreateTank2ProdList(fuelprices);

      _mops = fuelprices.Descendants("fuelMOP")
        .Select(mop => new MOP
        {
          MopId = Convert.ToInt32(mop.Attribute("sysid").Value),
          MopName = mop.Attribute("name").Value
        });

      _servicelvls = fuelprices.Descendants("fuelSvcMode")
        .Select(svclvl => new ServiceLevel
        {
          ServiceLevelId = Convert.ToInt32(svclvl.Attribute("sysid").Value),
          ServiceLevelName = svclvl.Attribute("name").Value
        });

      _productprice = fuelprices.Descendants("fuelProduct")
        .Where(prodprices => prodprices.Attribute("NAXMLFuelGradeID").Value != "0")
        .Where(prodprices => prodprices.Attribute("name").Value != "none")
        .Where(prodprices => !prodprices.Attribute("name").Value.Contains("INVALID"))
        .Where(prodprices => !prodprices.Attribute("name").Value.Contains("SKIP"))
        .Where(prodprices => !prodprices.Attribute("name").Value.Contains("NONE"))
        .Where(prodprices => !prodprices.Attribute("name").Value.Contains("UNUSED"))
        .Select(prodprices => new ProductPrice
        {
          ProductId = Convert.ToInt32(prodprices.Attribute("sysid").Value),
          ProductName = prodprices.Attribute("name").Value,
          Prices = prodprices.Element("prices")
            .Elements("price")
            .Where(price => Convert.ToInt32(price.Attribute("servLevel").Value) == 1 &&
                Convert.ToInt32(price.Attribute("tier").Value) == 1)
            .Select(price => new Price
            {
              MopLevelId = Convert.ToInt32(price.Attribute("mop").Value),
              ServiceLevelId = Convert.ToInt32(price.Attribute("servLevel").Value),
              Tier = Convert.ToInt32(price.Attribute("tier").Value),
              PriceLevel = Convert.ToDecimal(price.Value)
            }).ToList(),
          Blends = prodprices.Element("tanks")
            .Elements("tank")
            .Select(blend => new Blend
            {
              TankId = Convert.ToInt32(blend.Attribute("sysid").Value),
              BlendPercent = Convert.ToInt32(blend.Attribute("tankPercent").Value)
            }).ToList()
        }).ToList();

      SavePricesAndBlends();

      foreach (var pp in _productprice)
      {
        SysLog.LogTrace("prodid-{0} |name-{1}", pp.ProductId, pp.ProductName);
        //Trace.WriteLine("prodid-" + pp.ProductId + "|name-" + pp.ProductName);
        foreach (var p in pp.Prices)
        {
          SysLog.LogTrace("srvlevel-{0} |mopid-{1} |tier-{2} |price-{3}", p.ServiceLevelId, p.MopLevelId, p.Tier, p.PriceLevel);
          //Trace.WriteLine("srvlevel-" + p.ServiceLevelId + "|mopid-" + p.MopLevelId + "|tier-" + p.Tier + "|price- " + p.PriceLevel);
        }
        foreach (var b in pp.Blends)
        {
          try
          {
            b.PosProductId = pp.ProductId;
            b.AtgProductId = mytanks.First(t => t.TankId == b.TankId).ProductId;
            SysLog.LogTrace("tankid-{0} |atgproductId-{1} |posproductid-{2} |blend%-{3}", b.TankId,
              b.AtgProductId, b.PosProductId, b.BlendPercent);
          }
          catch (Exception e)
          {
            SysLog.LogError(e.Message);
          }
        }
      }
    }

    public List<TankId2ProdId> CreateTank2ProdList(XDocument fuelprices)
    {
      var mytanks = new List<TankId2ProdId>();

      List<ProductPrice> productprice = fuelprices.Descendants("fuelProduct")
      .Where(prodprices => prodprices.Attribute("NAXMLFuelGradeID").Value != "0")
      .Where(prodprices => !prodprices.Attribute("name").Value.Contains("INVALID"))
      .Where(prodprices => !prodprices.Attribute("name").Value.Contains("SKIP"))
      .Where(prodprices => !prodprices.Attribute("name").Value.Contains("NONE"))
      .Where(prodprices => !prodprices.Attribute("name").Value.Contains("UNUSED"))
      .Select(prodprices => new ProductPrice
      {
        ProductId = Convert.ToInt32(prodprices.Attribute("sysid").Value),
        ProductName = prodprices.Attribute("name").Value,
        Blends = prodprices.Element("tanks")
          .Elements("tank")
          .Where(blend => Convert.ToInt32(blend.Attribute("tankPercent").Value) != 100)
          .Select(blend => new Blend
          {
            TankId = Convert.ToInt32(blend.Attribute("sysid").Value),
            BlendPercent = Convert.ToInt32(blend.Attribute("tankPercent").Value)
          }).ToList()
      }).ToList();

      foreach (var pp in productprice)
      {
        foreach (var b in pp.Blends)
        {
          //          Trace.WriteLine("CreateTank2ProdList: tankid- " + b.TankId + "|prodid-" + pp.ProductId + "|ProductName-" + pp.ProductName + "|%-" + b.BlendPercent);
          SysLog.LogTrace("CreateTank2ProdList: tankid- {0}|prodid-{1}|ProductName-{2}|% -{3}", b.TankId, pp.ProductId, pp.ProductName, b.BlendPercent);
        }
      }

      productprice = fuelprices.Descendants("fuelProduct")
      .Where(prodprices => prodprices.Attribute("NAXMLFuelGradeID").Value != "0")
      .Where(prodprices => !prodprices.Attribute("name").Value.Contains("INVALID"))
      .Where(prodprices => !prodprices.Attribute("name").Value.Contains("SKIP"))
      .Where(prodprices => !prodprices.Attribute("name").Value.Contains("NONE"))
      .Where(prodprices => !prodprices.Attribute("name").Value.Contains("UNUSED"))
      .Select(prodprices => new ProductPrice
      {
        ProductId = Convert.ToInt32(prodprices.Attribute("sysid").Value),
        ProductName = prodprices.Attribute("name").Value,
        Blends = prodprices.Element("tanks")
          .Elements("tank")
          .Where(blend => Convert.ToInt32(blend.Attribute("tankPercent").Value) == 100)
          .Select(blend => new Blend
          {
            TankId = Convert.ToInt32(blend.Attribute("sysid").Value),
            BlendPercent = Convert.ToInt32(blend.Attribute("tankPercent").Value)
          }).ToList()
      }).ToList();

      foreach (var pp in productprice)
      {
        foreach (var b in pp.Blends)
        {
          //Trace.WriteLine("CreateTank2ProdList: tankid- " + b.TankId + "|prodid-" + pp.ProductId + "|ProductName-" + pp.ProductName);
          SysLog.LogTrace("CreateTank2ProdList: tankid- {0}|prodid-{1}|ProductName-{2}", b.TankId, pp.ProductId, pp.ProductName);
          var tank2prod = new TankId2ProdId();
          tank2prod.TankName = pp.ProductName;
          tank2prod.TankId = b.TankId;
          tank2prod.ProductId = pp.ProductId;
          mytanks.Add(tank2prod);
        }
      }

      foreach (var t in mytanks)
      {
        SysLog.LogTrace("TankName-{0} | TankId-{0} | ProdId-{1}", t.TankId, t.TankName, t.ProductId);
      }
      return mytanks;
    }

    public void ParsePrices(string datastream)
    {
      SysLog.LogTrace(datastream);
      var fuelprices = XDocument.Load(datastream);

      var productprice = fuelprices.Descendants("fuelProduct")
        .Where(prodprices => prodprices.Attribute("NAXMLFuelGradeID").Value != "0")
        .Where(prodprices => prodprices.Attribute("name").Value != "none")
        .Where(prodprices => !prodprices.Attribute("name").Value.Contains("INVALID"))
        .Where(prodprices => !prodprices.Attribute("name").Value.Contains("SKIP"))
        .Where(prodprices => !prodprices.Attribute("name").Value.Contains("NONE"))
        .Where(prodprices => !prodprices.Attribute("name").Value.Contains("UNUSED"))
        .Select(prodprices => new ProductPrice
        {
          ProductId = Convert.ToInt32(prodprices.Attribute("sysid").Value),
          ProductName = prodprices.Attribute("name").Value,
          Prices = prodprices.Element("prices")
            .Elements("price")
            .Where(price => Convert.ToInt32(price.Attribute("servLevel").Value) == 1 &&
                            Convert.ToInt32(price.Attribute("tier").Value) == 1)
            .Select(price => new Price
            {
              MopLevelId = Convert.ToInt32(price.Attribute("mop").Value),
              ServiceLevelId = Convert.ToInt32(price.Attribute("servLevel").Value),
              Tier = Convert.ToInt32(price.Attribute("tier").Value),
              PriceLevel = Convert.ToDecimal(price.Value)
            }).ToList()
        }).ToList();

      foreach (var pp in productprice)
      {
        SysLog.LogTrace("tankid-{0} |blend%-{1}", pp.ProductId, pp.ProductName);
        //Trace.WriteLine("prodid-" + pp.ProductId + "|name-" + pp.ProductName);
        foreach (var p in pp.Prices)
        {
          SysLog.LogTrace("srvlevel-{0} |mopid-{1} |tier-{2} |price-{3}", p.ServiceLevelId, p.MopLevelId, p.Tier, p.PriceLevel);
          //Console.WriteLine("srvlevel-" + p.ServiceLevelId + "|mopid-" + p.MopLevelId + "|tier-" + p.Tier + "|price- " + p.PriceLevel);
        }
      }
    }

    private void ParseTotalizers(string datastream)
    {
      var fuelTotals = XDocument.Parse(datastream);

      _totalizers = (from total in fuelTotals.Descendants("fpDispenserData")
                     select new Totalizer
                     {
                       Position = Convert.ToInt32(total.Element("fuelingPositionId").Value),
                       Product = total.Element("productNumber").FirstAttribute.Value,
                       ProductId = Convert.ToInt32(total.Element("productID").Value),
                       Volume = Convert.ToSingle(total.Element("fuelVolume").Value),
                       UofM = total.Element("fuelVolume").FirstAttribute.Value,
                       Money = Convert.ToSingle(total.Element("fuelMoney").Value),
                       Currency = total.Element("fuelMoney").FirstAttribute.Value
                     }).ToList();

      SaveTotalizers();
      foreach (var t in _totalizers)
      {
        SysLog.LogTrace("fuelpos-{0} |Prod#-{1} |ProdId-{2} |Volume-{3} |UofM-{4} |Currency-{5} |Money-{6}", t.Position, t.Product, t.ProductId, t.Volume, t.UofM, t.Currency, t.Money);
        //Trace.WriteLine("fuelpos-" + t.Position + "|Prod#-" + t.Product + "|ProdId-" + t.ProductId + "|Volume- " + t.Volume + "|UofM- " + t.UofM + "|Currency- " + t.Currency + "|Money- " + t.Money);
      }
      CalculatePosTotals();
      CalculateAtgTotals();
    }

    private void ParseRunningHoseTotal(string datastream)
    {
      try
      {
        _postotals = new List<PosTotal>();
        var xmlSerializer = new XmlSerializer(typeof(fpHoseRunningPd));
        using (var reader = new StringReader(datastream))
        {
          if (xmlSerializer.Deserialize(reader) is fpHoseRunningPd obj)
          {
            foreach (var prodtotal in obj.totals.byFuelProduct)
            {
              var item = new PosTotal
              {
                Name = prodtotal.fuelProdBase.name,
                ProdId = prodtotal.fuelProdBase.sysid,
                Amount = prodtotal.fuelInfo.amount,
                Volume = prodtotal.fuelInfo.volume
              };
              _postotals.Add(item);
              SysLog.LogTrace("Name={0} |Id={1} |Volume={2} |Amount={3}", item.Name, item.ProdId, item.Volume, item.Amount);
            }
            SavePosTotals();
            CalculateAtgTotals();
          }
          else
          {
            SysLog.LogError("XML Object is null in POS Totals.");
          }
        }
      }
      catch (Exception ex)
      {
        SysLog.LogError("{0}", ex.Message);
      }

    }

    private void ParseSummary(string datastream)
    {
      var sb = new StringBuilder();
      sb.Append(datastream);
      //string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
      //var directory = System.IO.Path.GetDirectoryName(path);
      string filePath = @"C:\Files\Summary.xml";

      //this code section write stringbuilder content to physical text file.
      using (StreamWriter swriter = new StreamWriter(filePath))
      {
        swriter.Write(sb.ToString());
      }
    }

    private void ParseHoseTest(string datastream)
    {
      var sb = new StringBuilder();
      sb.Append(datastream);
      //string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
      //var directory = System.IO.Path.GetDirectoryName(path);
      string filePath = @"C:\Files\HoseTest.xml";

      //this code section write stringbuilder content to physical text file.
      using (StreamWriter swriter = new StreamWriter(filePath))
      {
        swriter.Write(sb.ToString());
      }
    }



    private void SavePricesAndBlends()
    {
      try
      {
        QueueClient queueClient = new QueueClient(Properties.Settings.Default.PosDataQConnection, Properties.Settings.Default.PriceMsgQName);
        queueClient.CreateIfNotExists();
        // This mesage needs to be XML Data so it can be easily parsed to create an SMS Message by the Azure Function that will send it
        var jsonmsg = new PriceChangeMessage
        {
          SiteId = SiteId
        };
        bool bPriceHasChanged = false;

        SiteTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(inidata["SiteInfo"]["TimeZone"]);
        var currenttime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, SiteTimeZoneInfo);
        foreach (var pp in _productprice)
        {
          foreach (var p in pp.Prices)
          {
            var tmpsvclvl = _servicelvls.First(s => s.ServiceLevelId == p.ServiceLevelId);
            var tmpmop = _mops.First(m => m.MopId == p.MopLevelId);

            // Cash
            if (p.ServiceLevelId == 1 && p.MopLevelId == 1 && p.Tier == 1)
            {
              if (!inidata["Prices"].ContainsKey("Cash-" + pp.ProductName))
              {
                inidata["Prices"].AddKey("Cash-" + pp.ProductName, p.PriceLevel.ToString());
                inifile.WriteFile("MTI.ini", inidata);
                inifile.ReadFile("MTI.ini");
              }
              if (p.PriceLevel != Convert.ToDecimal(inidata["Prices"]["Cash-" + pp.ProductName]))
              {
                jsonmsg.PriceChanges.Add(AddPriceChangeMsg(pp.ProductName, tmpmop.MopName, p.PriceLevel, Convert.ToDecimal(inidata["Prices"]["Cash-" + pp.ProductName])));
                inidata["Prices"]["Cash-" + pp.ProductName] = p.PriceLevel.ToString();
                inifile.WriteFile("MTI.ini", inidata);
                inifile.ReadFile("MTI.ini");
                bPriceHasChanged = true;
              }
            }
            // Credit
            if ((p.ServiceLevelId == 1 && p.MopLevelId == 2 && p.Tier == 1))
            {
              if (!inidata["Prices"].ContainsKey("Credit-" + pp.ProductName))
              {
                inidata["Prices"].AddKey("Credit-" + pp.ProductName, p.PriceLevel.ToString());
                inifile.WriteFile("MTI.ini", inidata);
                inifile.ReadFile("MTI.ini");
              }
              if (p.PriceLevel != Convert.ToDecimal(inidata["Prices"]["Credit-" + pp.ProductName]))
              {
                jsonmsg.PriceChanges.Add(AddPriceChangeMsg(pp.ProductName, tmpmop.MopName, p.PriceLevel, Convert.ToDecimal(inidata["Prices"]["Credit-" + pp.ProductName])));
                inidata["Prices"]["Credit-" + pp.ProductName] = p.PriceLevel.ToString();
                inifile.WriteFile("MTI.ini", inidata);
                inifile.ReadFile("MTI.ini");
                bPriceHasChanged = true;
              }
            }
          }
          foreach (var b in pp.Blends)
          {
            /*UnitOfWork.ExecuteProc("FuelBlends_Insert",
              new SqlParameter("@SiteId", SiteId),
              new SqlParameter("@PosProductId", b.PosProductId),
              new SqlParameter("@AtgProductId", b.AtgProductId),
              new SqlParameter("@Percentage", b.BlendPercent));*/
          }
        }
        if (bPriceHasChanged)
        {
          var msg = JsonConvert.SerializeObject(jsonmsg);
          var response = queueClient.SendMessage(msg);
          SysLog.LogTrace("Message attempt response...{0}", response.GetRawResponse().ReasonPhrase);
        }
        else
          SysLog.LogInfo("No price changes detected");
      }
      catch (Exception ex)
      {
        SysLog.LogError("{0}", ex.Message);
      }
    }

    private void SavePosTotals()
    {
      try
      {
        var currenttime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, SiteTimeZoneInfo);
        foreach (var tot in _postotals)
        {
          /*UnitOfWork.ExecuteProc("POSTotals_Insert",
            new SqlParameter("@SiteId", SiteId),
            new SqlParameter("@DateAndTime", currenttime),
            new SqlParameter("@PosProductId", tot.ProdId),
            new SqlParameter("@VolumeTotal", tot.Volume),
            new SqlParameter("@SalesTotal", tot.Amount));*/
        }
      }
      catch (Exception ex)
      {
        SysLog.LogError("{0}", ex.Message);
      }
    }

    private void SaveTotalizers()
    {
      try
      {
        var currenttime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, SiteTimeZoneInfo);
        foreach (var tz in _totalizers)
        {
          /*UnitOfWork.ExecuteProc("DispenserTotalizers_Insert",
            new SqlParameter("@SiteId", SiteId),
            new SqlParameter("@DateAndTime", currenttime),
            new SqlParameter("@PosProductId", tz.ProductId),
            new SqlParameter("@FuelingPosition", tz.Position),
            new SqlParameter("@ProductName", tz.Product),
            new SqlParameter("@VolumeTotal", tz.Volume),
            new SqlParameter("@SalesTotal", tz.Money));*/
        }
      }
      catch (Exception ex)
      {
        SysLog.LogError("{0}", ex.Message);
      }
    }

    private void CalculatePosTotals()
    {
      try
      {
        /*UnitOfWork.ExecuteProc("PosProductTotalizers_Insert",
          new SqlParameter("@SiteId", SiteId));*/
      }
      catch (Exception ex)
      {
        SysLog.LogError("{0}", ex.Message);
      }
    }

    private void CalculateAtgTotals()
    {
      try
      {
      }
      catch (Exception ex)
      {
        SysLog.LogError("{0}", ex.Message);
      }
    }

    /// <summary>
    /// Send the Price Change Notification to pospricemsgq
    /// </summary>
    /// <param name="SiteId"></param>
    /// <param name="productName"></param>
    /// <param name="mopName"></param>
    /// <param name="currentPrice"></param>
    /// <param name="previousPrice"></param>
    private PriceChange AddPriceChangeMsg(string productName, string mopName, decimal currentPrice, decimal previousPrice)
    {
      var price = new PriceChange
      {
        ProductName = productName,
        MOPName = mopName,
        LastPrice = previousPrice,
        CurrentPrice = currentPrice
      };
      SysLog.LogTrace("Price Change added.. {0}", JsonConvert.SerializeObject(price));
      return price;
    }
  }
}
