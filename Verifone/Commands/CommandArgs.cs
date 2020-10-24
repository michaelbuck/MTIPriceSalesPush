using Chilkat;
using MTIPriceSalesPush.Shared;

namespace MTIPriceSalesPush.Verifone.Commands
{
  public class CommandArgs
  {
    public Socket Socket { get; set; }
    public string Url { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    public string Cookie { get; set; }
    public INLogger sysLog { get; set; }
    public int eventid { get; set; }
  }
}