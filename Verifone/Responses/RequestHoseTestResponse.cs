namespace MTIPriceSalesPush.Verifone.Responses
{
  public class RequestHoseTestResponse : Response
  {
    public string Payload { get; set; }

    public RequestHoseTestResponse()
    {

    }

    public RequestHoseTestResponse(string text) : base(text)
    {
      Payload = text;
    }

    public override string ToString()
    {
      return Payload;
    }
  }
}
