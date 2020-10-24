using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Storage.Queues;

namespace MTIPriceSalesPush
{
  public class MessageQ<T> where T : BaseMessage, new()
  {
    protected QueueClient queue;

    //--------------------------------------------------------------------------------
    public MessageQ(QueueClient queue)
    {
      this.queue = queue;
    }

    //--------------------------------------------------------------------------------
    public void AddMessage(T message)
    {
      var msg = new CloudQueueMessage(message.ToXml());
      queue.AddMessage(msg);
    }

    //--------------------------------------------------------------------------------
    public void DeleteMessage(CloudQueueMessage msg)
    {
      queue.DeleteMessage(msg);
    }

    //--------------------------------------------------------------------------------
    public CloudQueueMessage GetMessage()
    {
      return queue.GetMessage(TimeSpan.FromSeconds(60));
    }

    //--------------------------------------------------------------------------------
    public List<CloudQueueMessage> GetMessages(int messagesPerRequest)
    {
      return queue.GetMessages(messagesPerRequest, TimeSpan.FromSeconds(60)).ToList();
    }
  }
}