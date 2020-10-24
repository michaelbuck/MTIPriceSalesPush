namespace MTIPriceSalesPush
{
  /// <summary>
  /// Interface for a class that works with messages of type T in a queue.
  /// </summary>
  public interface IMessageQueue<T> : IMessageSender<T> where T : BaseMessage, new()
  {
    bool ProcessMessages();
  }
}