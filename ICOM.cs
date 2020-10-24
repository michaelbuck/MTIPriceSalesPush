namespace MTIPriceSalesPush
{
  public interface ICom
  {
    int BaudRate { get; set; }
    void ClearBuffer();
    void ClosePort();
    void SendVrCmd(string p);
    void SendVr250Cmd(string p);
    void Send(string p, bool bResponseExpected);
    void SendCmd(string p, bool bResponseExpected);
    void SendNativeModeCmd(string p);
    void SendPneumercatorCmd(string p);
    void SendRlm3Cmd(string p);

    OCom.CommsType ComType { get; set; }
    int DataBits { get; set; }
    System.IO.Ports.Handshake Handshake { get; set; }
    string HostName { get; set; }
    bool IsOpen();
    int LogLevel { get; set; }
    void OpenPort();
    System.IO.Ports.Parity Parity { get; set; }
    int Port { get; set; }
    string PortName { get; set; }
    string PortType { get; set; }
    System.IO.Ports.StopBits StopBits { get; set; }
    //string ThreadId { get; set; }
    double TimerInterval { get; set; }
    int BytesRcvd { get; set; }
    int BytesSent { get; set; }
    int EventId { get; set; }
    Chilkat.Socket Socket { get; set; }
    string SBuffer { get; set; }
    string EndOfCmd { get; set; }
    string Passcode { get; set; }    
  }
}