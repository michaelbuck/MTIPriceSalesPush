using System;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using MTIPriceSalesPush.Shared;

namespace MTIPriceSalesPush
{
  public abstract class MessageSenderBase<T> : IMessageSender<T> where T : BaseMessage, new()
  {
    protected int EventId { get; private set; }
    protected INLogger SysLog { get; private set; }
    protected MessageQ<T> Queue { get; private set; }
    private readonly string _logLevelParameterName;

    protected MessageSenderBase(string queueName, string logLevelParameterName, INLogger sysLog)
    {
      _logLevelParameterName = logLevelParameterName;
      SysLog = sysLog ?? new NLogger(GetType())
        {
          MinLogLevel = GetLogLevel()
        };

      try
      {
        var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings.Get("StorageConnectionString"));
        var queueClient = storageAccount.CreateCloudQueueClient();
        var cloudQueue = queueClient.GetQueueReference(queueName);
        cloudQueue.CreateIfNotExists();
        Queue = new MessageQ<T>(cloudQueue);
      }
      catch (Exception ex)
      {
        SysLog.LogError(ex);
        throw;
      }
    }

    protected int GetLogLevel()
    {
      return Convert.ToInt32(ConfigurationManager.AppSettings.Get("LogLevel"));
    }

    public virtual void AddMessage(T message)
    {
      SysLog.MinLogLevel = GetLogLevel();
      SysLog.LogDebug("Sending:{0}", message);

      Queue.AddMessage(message);
    }
  }
}