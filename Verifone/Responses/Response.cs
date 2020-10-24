using System;
using System.Text;
using System.Xml;

namespace MTIPriceSalesPush.Verifone.Responses
{
  public abstract class Response
  {
    public string Version { get; private set; }
    public DateTimeOffset TimeStampUtc { get; private set; }
    public DateTimeOffset TimeStampLocal { get; private set; }
    public string Key { get; private set; }
    public string Role { get; private set; }
    public string Command { get; private set; }
    public string Tag { get; private set; }
    public DateTimeOffset LastUpdated { get; private set; }

    protected XmlNode ResponseNode { get; private set; }

    protected Response()
    {
    }

    protected Response(string text) 
    {
    } 

    public override string ToString()
    {
      var builder = new StringBuilder();
      builder.AppendFormat("Version = {0}", Version).AppendLine();
      builder.AppendFormat("TimeStampUtc = {0}", TimeStampUtc).AppendLine();
      builder.AppendFormat("TimeStampLocal = {0}", TimeStampLocal).AppendLine();
      builder.AppendFormat("Key = {0}", Key).AppendLine();
      builder.AppendFormat("Role = {0}", Role).AppendLine();
      builder.AppendFormat("Command = {0}", Command).AppendLine();
      builder.AppendFormat("Tag = {0}", Tag).AppendLine();
      builder.AppendFormat("LastUpdated = {0}", LastUpdated).AppendLine();
      return builder.ToString();
    }
  }
}