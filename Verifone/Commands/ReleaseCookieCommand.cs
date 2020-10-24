using System;
using MTIPriceSalesPush.Verifone.Commands;
using MTIPriceSalesPush.Verifone.Responses;

namespace PosSocketTest.Commands
{
  public class ReleaseCookieCommand : Command
  {
    public ReleaseCookieCommand(CommandArgs args) : base(args)
    {
      Cmd = "releaseCredential";
      Cookie = args.Cookie;
    }

    protected override string BuildParameterList()
    {
      return string.Format("cmd={0}&cookie={1}", Cmd, Cookie);
    }

    protected override string BuildBody()
    {
      throw new NotImplementedException();
    }

    protected override Response ParseResponse(string text)
    {
      return new ReleaseCookieResponse(text);
    }
  }
}
