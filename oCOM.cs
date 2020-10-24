/******************************************************************************
 *   oCOM.cs
 *      Author:     Michael Buck
 *                  August 2009
 * 
 *      Description:  Communications Object - Handles both Serial and Socket connections transparently.
 *                    Set the CommsType property appropriately and use
 *      Notes on Operation:                   
 *              - This Class utilizes a System.Timer to enable poll timeouts.
 *              If SendCmd() is called the Timer will start. Any data recieved
 *              will reset the timer.  If the EndOfCmd character is not recieved
 *              and no data is recieved for TimerInterval(ms) then the Timer will
 *              fire and the 3 status flags:
 *                          EndOfCmd_Rcvd - EndofCmd string recieved
 *                          IsDone - Done recieving data
 *                          IsWaiting4Data - waiting for more data
 *              will be set appropriately for what has happened. The Timer is defaulted 
 *              to 3 sec to allow time for the command to be send, processed and a 
 *              response to begin.  This can be changed by setting the public property:
 *                  double TimerInterval = 
 *      TCPIP:  Default Port is 5600
 *      Serial: Default settings are 9600, 7 Odd 1
 *****************************************************************************/


using System;
using System.Text;
using System.IO.Ports;
using System.Linq;
using System.Timers;

namespace MTIPriceSalesPush
{
  public class OCom : ICom
  {
    public enum CommsType
    {
      Serial,
      Socket
    }

    // This timer allows the serial port to timeout when it is waiting for a response
    private Timer _serialTimer;

    public string SBuffer { get; set; }
    public byte[] ByteBuffer;
    public string EndOfCmd { get; set; }
    public string Passcode { get; set; }
    // Object State boolean variables
    public bool IsDone;
    public bool EndOfCmdRcvd;
    public bool IsWaiting4Data;
    // Byte Counters for Event Statistics
    public int BytesRcvd { get; set; }
    public int BytesSent { get; set; }
    // EventId MMG
    public int EventId { get; set; }

    private CommsType _comType;
    //
    //  Serial Port
    //
    private SerialPort _comPort;
    //
    //  Chilkat Socket
    //
    // Make the socket public so I can hand the inbound socket to the class
    public Chilkat.Socket Socket { get; set; }

    private const string Eot = "\x04";
    private const string Etx = "\x03";
    //private const String Lf = "\x0A";
    private const string Soh = "\x01";

    private string _hostName;
    private int _port;
    public NLogger SysLog;

    public OCom()
    {
      // Set the class defaults
      EndOfCmd = Etx;

      IsWaiting4Data = false;
      IsDone = false;
      EndOfCmdRcvd = false;
      PortType = "Socket";
      SysLog = new NLogger("oCOM");
    }

    #region Comm Object

    //
    //  Common Object Properties and Routines
    //
    public void OpenPort()
    {
      // Initialize the ByteCounters
      BytesRcvd = 0;
      BytesSent = 0;
      if (_comType == CommsType.Serial)
      {
        // Default to 10 sec (10000)
        _serialTimer = new Timer(10000);
        _serialTimer.Elapsed += SerialTimer_Elapsed;

        _comPort.Open();
        // Hook the Serial Port event Handler
        _comPort.DataReceived += _COMPort_DataReceived;
      }
      else // _COMType == CommsType.Socket
      {
        var success = Socket.Connect(_hostName, _port, false, 2000);
        // MB 3-27-2012  to alleviate data buffered at the remote device
        SBuffer = Socket.ReceiveString();
        ClearBuffer();
        if (success == false)
        {
          throw new Exception("Failed to Connect..." + Socket.LastErrorText);
        }
      }


      IsDone = false;
    }

    public void OpenSslPort()
    {
      // Initialize the ByteCounters
      BytesRcvd = 0;
      BytesSent = 0;
      var success = Socket.Connect(_hostName, _port, true, 2000);
      // To alleviate data buffered at the remote device
      SBuffer = Socket.ReceiveString();
      ClearBuffer();
      if (success == false)
      {
        throw new Exception("Failed to Connect..." + Socket.LastErrorText);
      }
      IsDone = false;
    }

    /*    public string ThreadId
        {
          get { return SysLog.ThreadId; }
          set { SysLog.ThreadId = value; }
        }*/

    public int LogLevel
    {
      get { return SysLog.MinLogLevel; }
      set { SysLog.MinLogLevel = value; }
    }

    public string PortType
    {
      get { return _comType.ToString(); }
      set { ComType = value == "Serial" ? CommsType.Serial : CommsType.Socket; }
    }

    public bool IsOpen()
    {
      return Socket.IsConnected;
    }

    public void ClosePort()
    {
      Socket.Close(1000);
    }

    public void ClearBuffer()
    {
      SBuffer = null;
    }

    public CommsType ComType
    {
      get { return _comType; }
      set
      { 
        // Set the Defaults specific to an IP Connection
        Socket = new Chilkat.Socket();
        Chilkat.Global glob = new Chilkat.Global();
        glob.UnlockBundle("MYTANK.CB1022021_GSgqBcCh6Hj3");
        _port = 5600;
        _hostName = "127.0.0.1";
        Socket.MaxReadIdleMs = 10000;
        Socket.MaxSendIdleMs = 10000;        
      }
    }

    // Port Timeout (when in data waiting mode)
    public double TimerInterval
    {
      set
      {
        if (_comType == CommsType.Serial)
        {
          _serialTimer.Interval = value;
        }
        else
        {
          Socket.MaxReadIdleMs = 30000;
          Socket.MaxSendIdleMs = 30000;
        }
      }
      get { return _comType == CommsType.Serial ? _serialTimer.Interval : Socket.MaxReadIdleMs; }
    }

    public void Send(string p, bool bResponseExpected)
    {
      if (_comType == CommsType.Serial)
      {
        if (_comPort.IsOpen)
        {
          _comPort.WriteLine(p);
        }
      }
      else
      {
        if (Socket.IsConnected)
        {
          ClearBuffer();
          Socket.SendString(p);
          SysLog.Write2Log(p, NLog.LogLevel.Trace, 0);
          BytesSent += p.Length;
          if (bResponseExpected)
            Wait4BinarySocketData();
        }
        else
          SysLog.Write2Log(p, NLog.LogLevel.Trace, 0);
      }
    }

    public void SendCmd(string p, bool bResponseExpected)
    {
      #region Serial

      if (_comType == CommsType.Serial)
      {
        ClearBuffer();
        IsWaiting4Data = true;
        IsDone = false;
        EndOfCmdRcvd = false;
        _serialTimer.Start();
        if (_comPort.IsOpen)
        {
          _comPort.WriteLine(p);
          BytesSent += p.Length + 1;
          while (IsWaiting4Data && !IsDone)
          {
            System.Threading.Thread.Sleep(1000);
          }
        }
      }
        #endregion

      else
      {
        ClearBuffer();
        IsDone = false;
        EndOfCmdRcvd = false;
        if (Socket.IsConnected)
        {
          ClearBuffer();
          Socket.SendString(p + "\r\n");
          SysLog.Write2Log(p, NLog.LogLevel.Trace, 0);
          BytesSent += p.Length + 1;
          if (bResponseExpected)
            Wait4SocketData();
        }
        else
          SysLog.Write2Log(p, NLog.LogLevel.Trace, 0);
      }
    }

    public void SendVrCmd(string p)
    {
      #region Serial

      if (_comType == CommsType.Serial)
      {
        ClearBuffer();
        IsWaiting4Data = true;
        IsDone = false;
        EndOfCmdRcvd = false;
        _serialTimer.Start();
        if (_comPort.IsOpen)
        {
          _comPort.WriteLine(Soh + p);
          BytesSent += p.Length + 2;
          while (IsWaiting4Data && !IsDone)
          {
            System.Threading.Thread.Sleep(1000);
          }
        }
      }
        #endregion

      else
      {
        ClearBuffer();
        IsWaiting4Data = true;
        IsDone = false;
        EndOfCmdRcvd = false;
        //SerialTimer.Start();
        if (Socket.IsConnected)
        {
          ClearBuffer();
          var data = (new StringBuilder(Soh + Passcode + p)).ToString();
          Socket.SendString(data);
          SysLog.Write2Log(data, NLog.LogLevel.Trace, 0);
          BytesSent += data.Length;
          Wait4SocketData();
          if (SBuffer.StartsWith(Soh)) return;
          var pos = SBuffer.IndexOf(Soh, StringComparison.Ordinal);
          SBuffer = SBuffer.Substring(pos, SBuffer.Length - pos);
        }
        else
        {
          SysLog.Write2Log("Socket NOT Connected..", NLog.LogLevel.Trace, 0);
        }
      }
    }

    public void SendVr250Cmd(string p)
    {
      #region Serial

      if (_comType == CommsType.Serial)
      {
        ClearBuffer();
        IsWaiting4Data = true;
        IsDone = false;
        EndOfCmdRcvd = false;
        _serialTimer.Start();
        if (_comPort.IsOpen)
        {
          _comPort.WriteLine(Soh + p);
          BytesSent += p.Length + 2;
          while (IsWaiting4Data && !IsDone)
          {
            System.Threading.Thread.Sleep(1000);
          }
        }
      }
        #endregion

      else
      {
        ClearBuffer();
        IsWaiting4Data = true;
        IsDone = false;
        EndOfCmdRcvd = false;
        //SerialTimer.Start();
        if (Socket.IsConnected)
        {
          ClearBuffer();
          var data = (new StringBuilder(Soh + Passcode + p)).ToString();
          Socket.SendString(data);
          SysLog.Write2Log(data, NLog.LogLevel.Trace, 0);

          BytesSent += data.Length;
          Wait4BinarySocketData();
        }
        else
        {
          SysLog.Write2Log("Socket NOT Connected..", NLog.LogLevel.Trace, 0);
        }
      }
    }

    public void SendRlm3Cmd(string p)
    {
      if (_comType == CommsType.Serial)
      {
        ClearBuffer();
        IsWaiting4Data = true;
        IsDone = false;
        EndOfCmdRcvd = false;
        _serialTimer.Start();
        if (!_comPort.IsOpen) return;
        p = p + Eot;
        p = p + CalcCheckSumString(p);
        _comPort.WriteLine(p);
        SysLog.Write2Log(p, NLog.LogLevel.Trace, 0);
        BytesSent += p.Length;
        while (IsWaiting4Data && !IsDone)
        {
          System.Threading.Thread.Sleep(1000);
        }
      }
      else
      {
        ClearBuffer();
        IsWaiting4Data = true;
        IsDone = false;
        EndOfCmdRcvd = false;
        if (!Socket.IsConnected) return;
        p = p + Eot;
        p = p + CalcCheckSumString(p);
        Socket.SendString(p);
        SysLog.Write2Log(p, NLog.LogLevel.Trace, 0);
        BytesSent += p.Length;
        Wait4Rlm3SocketData();
      }
    }

    public void SendNativeModeCmd(string p)
    {
      ClearBuffer();
      IsWaiting4Data = true;
      IsDone = false;
      EndOfCmdRcvd = false;

      ClearBuffer();
      IsWaiting4Data = true;
      IsDone = false;
      EndOfCmdRcvd = false;
      //SerialTimer.Start();
      var encoding = new ASCIIEncoding();
      var data = new StringBuilder();
      data.Append(Soh);
      data.Append(Passcode);
      data.Append(p);
      var bytes = encoding.GetBytes(data.ToString());

      var crc = new Crc16();
      var myCrc = crc.ComputeChecksum(bytes).ToString("X4");
      data.Append(myCrc);
      data.Append(Etx);
      var strdata = data.ToString();

      #region Serial

      if (_comType == CommsType.Serial)
      {
        ClearBuffer();
        IsWaiting4Data = true;
        IsDone = false;
        EndOfCmdRcvd = false;
        _serialTimer.Start();
        if (!_comPort.IsOpen) return;
        _comPort.WriteLine(strdata);
        BytesSent += strdata.Length;
        while (IsWaiting4Data && !IsDone)
        {
          System.Threading.Thread.Sleep(1000);
        }
      }
      #endregion

      else
      {
        if (Socket.IsConnected)
        {
          ClearBuffer();
          Socket.SendString(strdata);
          SysLog.Write2Log(strdata, NLog.LogLevel.Trace, 0);

          BytesSent += strdata.Length;
          Wait4BinarySocketData();
        }
        else
        {
          SysLog.Write2Log("Socket NOT Connected..", NLog.LogLevel.Trace, 0);
        }
      }
    }

    public void SendPneumercatorCmd(string p)
    {
      if (_comType == CommsType.Serial)
      {
        ClearBuffer();
        IsWaiting4Data = true;
        IsDone = false;
        EndOfCmdRcvd = false;
        _serialTimer.Start();
        if (!_comPort.IsOpen) return;
        var data = new StringBuilder();
        data.Append(Soh);
        data.Append(p);
        data.Append(XorSum(p));
        data.Append(Eot);
        _comPort.Write(p);
        SysLog.Write2Log(p, NLog.LogLevel.Trace, 0);
        BytesSent += p.Length;
        while (IsWaiting4Data && !IsDone)
        {
          System.Threading.Thread.Sleep(1000);
        }
      }
      else
      {
        ClearBuffer();
        IsWaiting4Data = true;
        IsDone = false;
        EndOfCmdRcvd = false;
        if (!Socket.IsConnected) return;
        var data = new StringBuilder();
        data.Append(Soh);
        data.Append(p);
        data.Append(XorSum(data.ToString()));
        data.Append(Eot);
        Socket.SendString(data.ToString());
        SysLog.Write2Log(data.ToString(), NLog.LogLevel.Trace, 0);
        BytesSent += data.Length;
        Wait4BinarySocketData();
      }
    }

    internal void Initialize()
    {
    }

    #endregion

    #region Serial Port

    /// <summary>
    /// 
    /// </summary>
    public int BaudRate
    {
      get { return _comPort.BaudRate; }
      set { _comPort.BaudRate = value; }
    }

    public string PortName
    {
      get { return _comPort.PortName; }
      set { _comPort.PortName = value; }
    }

    public StopBits StopBits
    {
      get { return _comPort.StopBits; }
      set { _comPort.StopBits = value; }
    }

    public int DataBits
    {
      get { return _comPort.DataBits; }
      set { _comPort.DataBits = value; }
    }

    public Parity Parity
    {
      get { return _comPort.Parity; }
      set { _comPort.Parity = value; }
    }

    public Handshake Handshake
    {
      get { return _comPort.Handshake; }
      set { _comPort.Handshake = value; }
    }

    private void _COMPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      // When data is rcvd stop the timer
      _serialTimer.Stop();
      BytesRcvd += _comPort.BytesToRead;
      SBuffer += _comPort.ReadExisting();
      // Check for EndOfCommand, if found the command is done
      if (SBuffer.Contains(EndOfCmd))
      {
        IsDone = true;
        EndOfCmdRcvd = true;
        IsWaiting4Data = false;
      }
      else
      {
        _serialTimer.Start();
      }
    }

    private void SerialTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      _serialTimer.Stop();
      IsDone = true;
      EndOfCmdRcvd = false;
      IsWaiting4Data = false;
    }

    #endregion

    #region Socket

    public string HostName
    {
      get { return _hostName; }
      set { _hostName = value; }
    }

    public int Port
    {
      get { return _port; }
      set { _port = value; }
    }

    private void Wait4SocketData()
    {
      SBuffer = Socket.ReceiveUntilMatch(EndOfCmd);
      IsDone = true;
      if (SBuffer != null)
        BytesRcvd += SBuffer.Length;
      SysLog.Write2Log(SBuffer, NLog.LogLevel.Trace, 0);
      EndOfCmdRcvd = !string.IsNullOrEmpty(SBuffer);
    }

    private void Wait4Rlm3SocketData()
    {
      SBuffer = Socket.ReceiveUntilMatch(Eot);
      SBuffer = SBuffer + Socket.ReceiveString();
      IsDone = true;
      if (SBuffer != null)
        BytesRcvd += SBuffer.Length;
      SysLog.Write2Log(SBuffer, NLog.LogLevel.Trace, 0);
      EndOfCmdRcvd = !string.IsNullOrEmpty(SBuffer);
    }

    private void Wait4BinarySocketData()
    {
      SBuffer = "";
      var bNotDone = true;
      var sbBuffer = new StringBuilder();
      // Must time out of this to close the port
      // If data is sent and none is recieved
      var countDown = DateTime.Now;
      while (bNotDone)
      {
        if (Socket.PollDataAvailable())
        {
          ByteBuffer = Socket.ReceiveBytes();
          countDown = DateTime.Now;
          for (var i = 0; i < ByteBuffer.Length; i++)
          {
            if (ByteBuffer[i] == 0)
              ByteBuffer[i] = 32;
          }
          sbBuffer.Append(Encoding.ASCII.GetString(ByteBuffer));
        }
        if (DateTime.Now <= countDown.AddSeconds(5)) continue;
        SysLog.Write2Log("Wait4BinarySocketData timed out...", NLog.LogLevel.Trace, 0);
        bNotDone = false;
      }

      IsDone = true;
      SBuffer = sbBuffer.ToString();
      if (SBuffer != null)
        BytesRcvd += SBuffer.Length;

      SysLog.Write2Log(SBuffer, NLog.LogLevel.Trace, 0);
      EndOfCmdRcvd = !string.IsNullOrEmpty(SBuffer);
    }

    #endregion

    #region CRC16

    public class Crc16
    {
      private const ushort Poly = 4129;
      private ushort[] table = new ushort[256];
      private ushort initialValue;

      public ushort ComputeChecksum(byte[] bytes)
      {
        var crc = initialValue;
        for (var i = 0; i < bytes.Length; ++i)
        {
          crc = (ushort) ((crc << 8) ^ table[((crc >> 8) ^ (0xff & bytes[i]))]);
        }
        return crc;
      }

      public byte[] ComputeChecksumBytes(byte[] bytes)
      {
        var crc = ComputeChecksum(bytes);
        return BitConverter.GetBytes(crc);
      }

      public Crc16()
      {
        initialValue = 0xFFFF;
        for (var i = 0; i < table.Length; ++i)
        {
          ushort temp = 0;
          var a = (ushort) (i << 8);
          for (var j = 0; j < 8; ++j)
          {
            if (((temp ^ a) & 0x8000) != 0)
            {
              temp = (ushort) ((temp << 1) ^ Poly);
            }
            else
            {
              temp <<= 1;
            }
            a <<= 1;
          }
          table[i] = temp;
        }
      }
    }

    #endregion

    #region CheckSum
    private static string CalcCheckSumString(string data)
    {
      var array = Encoding.ASCII.GetBytes(data);

      var chkSum = array.Aggregate(0, (s, b) => s += b);
      chkSum = (0x0000 - chkSum) & 0x7f;

      var cs = Encoding.ASCII.GetString(BitConverter.GetBytes(chkSum));
      cs = cs.Substring(0, 1);
      return cs;
    }
    #endregion

    #region XOR Sum Calculation (Pneumercator)

    private static string XorSum(string data)
    {
      var byteData = Encoding.ASCII.GetBytes(data);
      byte chkSumByte = 0x55;
      for (int i = 0; i < byteData.Length; i++)
        chkSumByte ^= byteData[i];

      StringBuilder sb = new StringBuilder();
      sb.AppendFormat("{0:x2}", chkSumByte);

      return sb.ToString();
    }
    #endregion
  }
}