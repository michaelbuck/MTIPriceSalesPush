namespace MTIPriceSalesPush
{
  /// <summary>
  /// Interface for a class that sends messages to a queue.
  /// </summary>
  /// <typeparam name="T">Type of message to send.</typeparam>
  public interface IMessageSender<T> where T : BaseMessage, new()
  {
    /// <summary>
    /// Adds the given message to the queue.
    /// </summary>
    /// <param name="message">Message to send.</param>
    void AddMessage(T message);
  }
}