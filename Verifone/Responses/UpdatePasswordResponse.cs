namespace MTIPriceSalesPush.Verifone.Responses
{
  public class UpdatePasswordResponse : Response
  {
    public string Payload { get; set; }

    public UpdatePasswordResponse()
    {
    }

    public UpdatePasswordResponse(string text): base(text)
    {
      Payload = text;
    }

    public override string ToString()
    {
      return Payload;
    }
  }
}
