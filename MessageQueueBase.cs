using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace MTIPriceSalesPush
{
  public abstract class MessageQueueBase<T> : IMessageQueue<T> where T : BaseMessage, new()
  {
    protected INLogger SqLogger { get; private set; }
    protected string ConfigPrefix { get; private set; }

    private readonly CloudStorageAccount _storageAccount;
    private readonly CloudQueueClient _queueClient;
    private readonly CloudQueue _cloudQueue;
    private readonly MessageQ<T> _queue;
    //private int _messagesPerRequest;

    protected MessageQueueBase(string configPrefix, string queueName, INLogger sqLogger = null)
    {      
      ConfigPrefix = configPrefix;
      SqLogger = sqLogger ?? new NLogger(GetType().Name);

      try
      {
        //GetVolatileConfig();

        _storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.PosDataQConnection);
        _queueClient = _storageAccount.CreateCloudQueueClient();
        _cloudQueue = _queueClient.GetQueueReference(queueName);
        _cloudQueue.CreateIfNotExists();

        _queue = new MessageQ<T>(_cloudQueue);
      }
      catch (Exception ex)
      {
        SqLogger.LogError(ex);
      }
    }

    /// <summary>
    /// Loads configuration for settings we can expect to change often.
    /// </summary>
    //private void GetVolatileConfig()
    //{
    //  _messagesPerRequest = ConfigurationManager.AppSettings["GetQueueMessagesPerRequest"];
    //  SqLogger.MinLogLevel = ConfigurationManager.AppSettings["GetMinLogLevel"];
    //}

    public virtual void AddMessage(T message)
    {
      try
      {
        //GetVolatileConfig();

        _queue.AddMessage(message);
      }
      catch (Exception ex)
      {
        SqLogger.LogError(ex);
        throw;
      }
    }

    public virtual bool ProcessMessages()
    {
      try
      {
        //GetVolatileConfig();

        var queueMessages = _queue.GetMessages(5);
        if (queueMessages == null || queueMessages.Count == 0)
        {
          SqLogger.LogTrace("ProcessMessages():MessageCount=0");
          return false;
        }

        SqLogger.LogTrace("ProcessMessages():MessageCount={0}", queueMessages.Count);
        foreach (var queueMessage in queueMessages)
        {
          if (queueMessage == null) continue;
          var message = BaseMessage.FromXmlMessage<T>(queueMessage);
          SqLogger.LogTrace("ProcessMessages():{0}", message);

          ProcessMessage(message);

          _queue.DeleteMessage(queueMessage);
        }
        return true;
      }
      catch (Exception ex)
      {
        SqLogger.LogError(ex);
        return false;
      }
    }

    protected abstract void ProcessMessage(T siteMessage);
  }
}