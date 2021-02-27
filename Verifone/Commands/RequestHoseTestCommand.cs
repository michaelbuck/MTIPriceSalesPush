using MTIPriceSalesPush.Verifone.Responses;

namespace MTIPriceSalesPush.Verifone.Commands
{
  public class RequestHoseTestCommand : Command
  {
    private readonly string _period;
    private readonly string _reptnum;
    public RequestHoseTestCommand(CommandArgs args) : base(args)
    {
      Cmd = "vrubyrept&reptname=hoseTest";
      _period = "2";
      _reptnum = "2";
      Cookie = args.Cookie;
    }

    protected override string BuildParameterList()
    {
      return $"cmd={Cmd}&period={_period}&reptnum={_reptnum}&cookie={Cookie}";
    }

    protected override string BuildBody()
    {
      throw new System.NotImplementedException();
    }

    protected override Response ParseResponse(string text)
    {
      return !NewAuthRequired(text) ? new RequestHoseTestResponse(text) : null;
    }
  }
}
