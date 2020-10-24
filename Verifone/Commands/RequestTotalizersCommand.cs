using MTIPriceSalesPush.Verifone.Responses;

namespace MTIPriceSalesPush.Verifone.Commands
{
  public class RequestTotalizersCommand : Command
  {
    private readonly string _period;
    private readonly string _filename;
    public RequestTotalizersCommand(CommandArgs args) : base(args)
    {
      Cmd = "vrubyrept&reptname=fpHoseRunning";
      //Cmd = "vfueltotals";
      _period = "1";
      _filename = "current";
      Cookie = args.Cookie;
    }

    protected override string BuildParameterList()
    {
      return $"cmd={Cmd}&period={_period}&filename={_filename}&cookie={Cookie}";
    }

    protected override string BuildBody()
    {
      throw new System.NotImplementedException();
    }

    protected override Response ParseResponse(string text)
    {
      return !NewAuthRequired(text) ? new RequestTotalizersResponse(text) : null;
    }
  }
}
