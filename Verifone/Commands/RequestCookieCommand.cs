using System.Xml;
using System.Xml.Linq;
using MTIPriceSalesPush.Verifone.Responses;

namespace MTIPriceSalesPush.Verifone.Commands
{
  public class RequestCookieCommand : Command
  {
    public RequestCookieCommand(CommandArgs args) : base(args)
    {
      Cmd = "validate";
    }

    protected override string BuildParameterList()
    {
      return $"cmd={Cmd}&user={User}&passwd={Password}";
    }

    protected override string BuildBody()
    {
      throw new System.NotImplementedException();
    }

    protected override Response ParseResponse(string text)
    {
      if (UsernamePasswordNotValid(text)) return null;
      // Get the Cookie and assign to the base Command variable "Cookie"
      var xmlDoc = new XmlDocument();
      xmlDoc.LoadXml(text);
      var elements = xmlDoc.GetElementsByTagName("cookie");
      Cookie = elements[0].InnerText;
      return new RequestCookieResponse(text);
    }

    private bool UsernamePasswordNotValid(string text)
    {
      var errchk = XDocument.Parse(text);
      var root = (string)errchk.Root;
      return root.Contains("Invalid Username or Password");
    }
  }
}