namespace PacketSimulatorServerWorkerService;

public class PacketServerSettings
{
    public string IpAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; }
    public string StartByteHex { get; set; } = "D1";
    public int Offset { get; set; }
    public int ReadRange { get; set; }
    public bool IsLittleEndian { get; set; } = true;
}