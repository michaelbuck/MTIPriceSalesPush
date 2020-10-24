using System;
using System.IO;
using System.Text;
using System.Xml;
using MTIPriceSalesPush.Verifone.Responses;

namespace MTIPriceSalesPush.Verifone.Commands
{  
  public class UpdatePasswordCommand : Command
  {
    private const string CRLFCRLF = "\r\n\r\n";
    //private string _payload;
    private string _url;
    private readonly string _password;
    private readonly string _username;
    public string Newpassword;
    public UpdatePasswordCommand(CommandArgs args) : base(args)
    {
      Cmd = "changepasswd";
      Cookie = args.Cookie;
      _password = args.Password;
      _username = args.User;
      _url = args.Url;
    }

    protected override string BuildParameterList()
    {      
      return $"{_url}cmd={Cmd}&cookie={Cookie}";
    }

    protected override string BuildBody()
    {
      Newpassword = NewPassword(_password);
      return PasswordCfgXmlString();
    }

    protected override Response ParseResponse(string text)
    {
      return !NewHttpAuthRequired(text) ? new UpdatePasswordResponse(text) : null;
    }

    private string PasswordCfgXmlString()
    {
      using (var sw = new StringWriter())
      {
        Encoding utf8NoBom = new UTF8Encoding(false);
        var settings = new XmlWriterSettings
        {
          Indent = false,
          Encoding = utf8NoBom
        };
        using (var output = new MemoryStream())
        {
          using (var writer = XmlWriter.Create(output, settings))
          {
            writer.WriteStartDocument();
            writer.WriteStartElement("domain","passwdConfig", "urn:vfi-sapphire:np.domain.2001-07-01");
            writer.WriteStartElement("user");
            writer.WriteAttributeString("name", _username);
            writer.WriteStartElement("passwd");
            writer.WriteAttributeString("newValue", NewPassword(_password));
            writer.WriteAttributeString("oldValue", _password);
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
          }
          return Encoding.Default.GetString(output.ToArray());
        }
      }
    }

    private static string NewPassword(string currentpswd)
    {
      // Initial password a000000  -- character first (1), numbers last (6)
      // will increment a password letters and numbers of the length passed in
      // Numbers will increment first. once 9 is reached on all numbers then
      // the alpha part will increment and numbers will be all 0s
      // once the first char reaches z and numeric reaches 999999 then
      // the password will reset back to a + the numbers of 0s in the initial password

      var newpswd = "";
      var newnumpart = "";
      var newalphapart = "";
      var pswdlen = currentpswd.Length;
      // separate the alpha beginning from the numerical end
      var numpos = currentpswd.IndexOfAny("0123456789".ToCharArray());
      var numpart = currentpswd.Substring(numpos, currentpswd.Length - numpos);
      var alphapart = currentpswd.Substring(0, numpos);
      var inumpart = int.Parse(numpart);

      // Generate the max number to compare the Numerical part to
      var maxnumpart = "";
      maxnumpart = maxnumpart.PadLeft(currentpswd.Length - numpos, '9');
      var imaxnumpart = int.Parse(maxnumpart);
      if (imaxnumpart == inumpart) // We have reached the max number
      {
        var tmpnumpart = "";
        // reset the numpart to 0s but 1 number less.  we are going to add an 'a' to end of the alpha part
        newnumpart = tmpnumpart.PadLeft(currentpswd.Length - numpos, '0');
        var tmpalphapart = newalphapart.PadLeft(alphapart.Length, 'z');
        newalphapart = alphapart == tmpalphapart ? newalphapart.PadLeft(alphapart.Length, 'a') : Convert.ToChar(alphapart[0] + 1).ToString();
      }
      else
      {
        inumpart++;
        newnumpart = inumpart.ToString();
        newnumpart = newnumpart.PadLeft(numpart.Length, '0');
        newalphapart = alphapart;
      }
      newpswd = newalphapart + newnumpart;
      return newpswd;
    }
  }
}
