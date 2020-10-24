namespace MTIPriceSalesPush
{
  public interface IIEvent
  {
    void StartEvent();
    void EndEvent();
    void OpenPort();
    void ClosePort();
    void RunEvent();
  }
}
