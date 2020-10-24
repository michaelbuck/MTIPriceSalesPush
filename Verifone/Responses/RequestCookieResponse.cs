using System.Xml;

namespace MTIPriceSalesPush.Verifone.Responses
{
  public class RequestCookieResponse : Response
  {
    public string Cookie { get; set; }
    public string Payload { get; set; }
  

    public RequestCookieResponse()
    {
      Cookie = "";
    }

    public RequestCookieResponse(string text) : base(text)
    {
      Payload = text;
      var xmlDoc = new XmlDocument();
      xmlDoc.LoadXml(text);
      var node = xmlDoc;
      Cookie = node.GetAttributeString("cookie");
    }

    public override string ToString()
    {
      return Payload;
    }
  }
}