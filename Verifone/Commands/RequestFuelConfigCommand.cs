using System;
using MTIPriceSalesPush.Verifone.Responses;

namespace MTIPriceSalesPush.Verifone.Commands
{
  public class RequestFuelConfigCommand: Command
  {
    public RequestFuelConfigCommand(CommandArgs args) : base(args)
    {
      Cmd = "vfuelcfg";
      Cookie = args.Cookie;
    }

    protected override string BuildParameterList()
    {
      return $"cmd={Cmd}&cookie={Cookie}";
    }

    protected override string BuildBody()
    {
      throw new NotImplementedException();
    }

    protected override Response ParseResponse(string text)
    {
      return !NewAuthRequired(text) ? new RequestFuelCfgResponse(text) : null;
    }
  }
}
