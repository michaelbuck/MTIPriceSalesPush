namespace MTIPriceSalesPush.Verifone.Responses
{
  public class RequestFuelCfgResponse : Response
  {
    public string Payload { get; set; }

    public RequestFuelCfgResponse()
    {
      
    }

    public RequestFuelCfgResponse(string text) : base(text)
    {
      Payload = text;
    }

    public override string ToString()
    {
      return Payload;
    }
  }
}
