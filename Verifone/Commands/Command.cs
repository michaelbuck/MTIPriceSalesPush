using System;
using System.Text;
using System.Xml.Linq;
using Chilkat;
using MTIPriceSalesPush.Verifone.Responses;
using StringBuilder = System.Text.StringBuilder;
using MTIPriceSalesPush.Shared;

namespace MTIPriceSalesPush.Verifone.Commands
{
  public abstract class Command : ICommand
  {
    protected Socket Socket { get; private set; }
    protected Uri Uri { get; private set; }
    protected string Url { get; set; }
    protected string Password { get; private set; }
    protected string User { get; private set; }
    public string Cmd { get; set; }
    public string Cookie { get; set; }
    public INLogger SysLog;
    public int Eventid;

    protected Command(CommandArgs args)
    {
      //Uri = uri;
      Socket = args.Socket;
      Password = args.Password;
      User = args.User;
      Url = args.Url;
      SysLog = args.sysLog;
      Eventid = args.eventid;
    }

    public Response Execute()
    {
      var parameterlist = BuildParameterList();
      var requestText = GetHttpRequestText(parameterlist);      
      SysLog.LogTrace("{0}",requestText);
      Socket.SendString(requestText);
      
       var header = ParseHeader();
       return ParseResponse(header);
    }

    public Response ExecutePost()
    {
      var parameterlist = BuildParameterList();
      var body = BuildBody();
      var req = new Chilkat.HttpRequest
      {
        HttpVerb = "POST",
        Path = parameterlist,
        ContentType = "application/xml"
      };
      //  The ContentType, HttpVerb, and Path properties should always be explicitly set.
      req.LoadBodyFromString(body , "utf-8");
      var requestText = req.GenerateRequestText();

      SysLog.LogTrace("{0}", requestText);
      Socket.SendString(requestText);

      var header = ParseHeader();
      return ParseResponse(header);
    }

    protected abstract string  BuildParameterList();
    protected abstract string BuildBody();
    protected abstract Response ParseResponse(string text);

    private string GetHttpRequestText(string paramlist)
    {   
      var builder = new StringBuilder();
      builder.Append("GET ").Append(Url);    
      builder.Append(paramlist);
      builder.AppendLine();
      builder.AppendLine();
      return builder.ToString();
    }

    private string ParseHeader()
    {
      var bNotDone = true;
      var sbBuffer = new StringBuilder();
      // Must time out of this to close the port
      // If data is sent and none is recieved
      var countDown = DateTime.Now;
      while (bNotDone)
      {
        if (Socket.PollDataAvailable())
        {
          var bbuffer = Socket.ReceiveBytes();
          countDown = DateTime.Now;
          sbBuffer.Append(Encoding.ASCII.GetString(bbuffer));
        }
        if (DateTime.Now > countDown.AddSeconds(2))
        {
          bNotDone = false;
        }
      }
      var header = sbBuffer.ToString();
      SysLog.LogTrace("{0}",header);
      //Console.WriteLine("{0}", header);
      //if (String.IsNullOrWhiteSpace(header)) throw new ApplicationException("No Response");
      return header;
    }

    protected bool NewAuthRequired(string text)
    {
      var errchk = XDocument.Parse(text);
      var root = (string)errchk.Root;
      if (root.Contains("No Credential"))
      {
        SysLog.LogError("No Credentials..{0}", root);
      }
      return root.Contains("No Credential");
    }

    protected bool NewHttpAuthRequired(string text)
    {
      var response = new HtmlToXml();
      response.Html = text ;
      var xml = response.ToXml();
      var errchk = XDocument.Parse(xml);      
      var root = (string)errchk.Root;
      if (root.Contains("No Credential"))
      {
        SysLog.LogError("No Credentials..{0}",root);
      }      
      return root.Contains("No Credential");
    }


  }
}