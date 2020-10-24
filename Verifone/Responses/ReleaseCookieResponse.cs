namespace MTIPriceSalesPush.Verifone.Responses
{
  public class ReleaseCookieResponse : Response
  {
    public string Cookie { get; set; }

    public ReleaseCookieResponse()
    {
      Cookie = "";
    }

    public ReleaseCookieResponse(string text) : base(text)
    {
      Cookie = text;
    }

    public override string ToString()
    {
      return string.Format("Cookie = {0}", Cookie);
    }
  }
}
