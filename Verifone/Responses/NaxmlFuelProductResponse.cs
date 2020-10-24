namespace MTIPriceSalesPush.Verifone.Responses
{
  class NaxmlFuelProductResponse : Response
  {
    public string NaxmlFuelProduct { get; set; }

    public NaxmlFuelProductResponse()
    {
      NaxmlFuelProduct = "";
    }

    public NaxmlFuelProductResponse(string text): base(text)
    {
      NaxmlFuelProduct = text;
    }

    public override string ToString()
    {
      return NaxmlFuelProduct;
    }
  }
}
