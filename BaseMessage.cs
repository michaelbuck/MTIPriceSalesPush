using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace MTIPriceSalesPush
{
  [Serializable]
  public abstract class BaseMessage
  {
    public string ToXml()
    {
      var m = new MemoryStream();
      var xs = new XmlSerializer(GetType());
      xs.Serialize(m, this);
      return Encoding.ASCII.GetString(m.ToArray());
    }

    public static T FromXmlMessage<T>(CloudQueueMessage m)
    {      
      var ms = new MemoryStream(GetBytes(m.AsString)) {Position = 0};
      var xs = new XmlSerializer(typeof (T));
      return (T) xs.Deserialize(ms);
    }

    public byte[] ToBinary()
    {
      var bf = new BinaryFormatter();
      byte[] output = null;
      using (var ms = new MemoryStream())
      {
        ms.Position = 0;
        bf.Serialize(ms, this);
        output = ms.GetBuffer();
      }
      return output;
    }

    public static T FromBinaryMessage<T>(CloudQueueMessage m)
    {
      var buffer = m.AsBytes;
      var returnValue = default(T);
      using (var ms = new MemoryStream(buffer))
      {
        ms.Position = 0;
        var bf = new BinaryFormatter();
        returnValue = (T) bf.Deserialize(ms);
      }
      return returnValue;
    }

    static byte[] GetBytes(string str)
    {
      var bytes = new byte[str.Length * sizeof(char)];
      System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
      return bytes;
    }
  }
}