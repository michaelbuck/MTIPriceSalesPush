using MTIPriceSalesPush.Verifone.Responses;

namespace MTIPriceSalesPush.Verifone.Commands
{
  public interface ICommand
  {
    Response Execute();
  }
}