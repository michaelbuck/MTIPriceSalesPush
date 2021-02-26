namespace MTIPriceSalesPush.Verifone.Responses
{
  public class RequestSummaryResponse : Response
  {
    public string Payload { get; set; }

    public RequestSummaryResponse()
    {

    }

    public RequestSummaryResponse(string text) : base(text)
    {
      Payload = text;
    }

    public override string ToString()
    {
      return Payload;
    }
  }
}
