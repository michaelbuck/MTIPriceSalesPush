namespace MTIPriceSalesPush.Verifone.Responses
{
  public class RequestTotalizersResponse : Response
  {
    public string Payload { get; set; }

    public RequestTotalizersResponse()
    {
      
    }

    public RequestTotalizersResponse(string text): base(text)
    {
      Payload = text;
    }

    public override string ToString()
    {
      return Payload;
    }
  }
}
