using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PacketSimulatorServer;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace PacketSimulatorServerWorkerService;

// 1. 기존에 만든 구조체 그대로 사용
public struct RentedPacket
{
    public byte[] Data;
    public int Length;
}

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly PacketServerSettings _settings;
    private readonly PacketStore _packetStore;

    private TcpListener _listener;
    private Channel<RentedPacket> _packetChannel;
    private byte _startByte;

    // 생성자: 의존성 주입을 통해 로거와 설정값을 받아옵니다.
    public Worker(ILogger<Worker> logger, IOptions<PacketServerSettings> options, PacketStore packetStore)
    {
        _logger = logger;
        _settings = options.Value; // 텍스트박스를 대체할 설정값들
        _packetStore = packetStore;

        // 2. ServerForm 생성자에 있던 Channel 셋업 그대로 이식
        var channelOptions = new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        };
        _packetChannel = Channel.CreateBounded<RentedPacket>(channelOptions);

        // 3. StartByteHex 파싱 ("D1" -> byte)
        _startByte = Convert.ToByte(_settings.StartByteHex, 16);
    }

    // 4. 기존 StartServer() 의 역할을 BackgroundService의 진입점이 대신합니다.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("패킷 서버를 시작합니다. IP: {IP}, Port: {Port}", _settings.IpAddress, _settings.Port);

        _listener = new TcpListener(IPAddress.Parse(_settings.IpAddress), _settings.Port);
        _listener.Start();

        // 소비자(Consumer) 태스크 시작
        _ = Task.Run(() => ConsumePacketsAsync(stoppingToken), stoppingToken);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync(stoppingToken);
                _logger.LogInformation("클라이언트 접속: {Client}", client.Client.RemoteEndPoint);

                _ = Task.Run(() => HandleClientAsync(client, stoppingToken), stoppingToken);
            }
        }
        catch (OperationCanceledException) { /* 정상 종료 */ }
        catch (Exception ex)
        {
            _logger.LogError(ex, "서버 수신 대기 중 예외 발생");
        }
        finally
        {
            _listener?.Stop();
        }
    }

    // 5. HandleClientAsync는 UI 종속성이 없으므로 거의 100% 그대로 복사합니다.
    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using (client)
        using (NetworkStream stream = client.GetStream())
        {
            byte[] readBuffer = ArrayPool<byte>.Shared.Rent(8192);
            List<byte> accumBuffer = new List<byte>();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(readBuffer, 0, readBuffer.Length, token);
                    if (bytesRead == 0) break;

                    for (int i = 0; i < bytesRead; i++) accumBuffer.Add(readBuffer[i]);

                    while (accumBuffer.Count > 0)
                    {
                        int packetLength = TryParsePacketLength(accumBuffer);
                        if (packetLength == 0) break;
                        else if (packetLength == -1)
                        {
                            int startIndex = accumBuffer.IndexOf(_startByte);
                            if (startIndex == -1) { accumBuffer.Clear(); break; }
                            else { accumBuffer.RemoveRange(0, startIndex); continue; }
                        }

                        byte[] packetData = ArrayPool<byte>.Shared.Rent(packetLength);
                        accumBuffer.CopyTo(0, packetData, 0, packetLength);
                        accumBuffer.RemoveRange(0, packetLength);

                        await _packetChannel.Writer.WriteAsync(new RentedPacket { Data = packetData, Length = packetLength }, token);
                    }
                }
            }
            catch { /* 통신 에러 무시 */ }
            finally { ArrayPool<byte>.Shared.Return(readBuffer); }
        }
    }

    // 6. TryParsePacketLength 역시 기존 코드 그대로 사용합니다. (설정값은 _settings 객체에서 가져옴)
    private int TryParsePacketLength(List<byte> buffer)
    {
        int startIndex = buffer.IndexOf(_startByte);
        if (startIndex == -1) return -1;
        if (startIndex > 0) return -1;

        int lengthIndex = startIndex + _settings.Offset;
        if (lengthIndex + _settings.ReadRange > buffer.Count) return 0;

        byte[] lengthBytes = new byte[_settings.ReadRange];
        buffer.CopyTo(lengthIndex, lengthBytes, 0, _settings.ReadRange);

        // 기존에 사용하시던 HelperLibrary 호출
        int packetLength = PacketHelper.GetLengthFromBytes(lengthBytes, _settings.IsLittleEndian);

        if (packetLength <= 0) return -1;
        if (buffer.Count < packetLength) return 0;

        return packetLength;
    }

    // 7. ConsumePacketsAsync: UI 그리는 부분을 로그 출력으로 대체합니다.
    private async Task ConsumePacketsAsync(CancellationToken token)
    {
        try
        {
            await foreach (RentedPacket packet in _packetChannel.Reader.ReadAllAsync(token))
            {
                // 기존 AddPacketToListBox 역할을 _logger.LogInformation이 대신합니다.
                // 빌려온 Data 배열은 8192 등 넉넉한 크기이므로, 유효한 Length까지만 잘라서 문자열로 만듭니다.
                string hexString = BitConverter.ToString(packet.Data, 0, packet.Length).Replace("-", " ");

                // 1. 기존처럼 파일에 로그 남기기
                _logger.LogInformation("[패킷 수신 완료] {HexString}", hexString);

                // 2. 웹에서 볼 수 있도록 큐에 저장하기 (추가!)
                _packetStore.AddPacket($"[{DateTime.Now:HH:mm:ss}] {hexString}");

                ArrayPool<byte>.Shared.Return(packet.Data);
            }
        }
        catch (OperationCanceledException) { }
    }
}

public class PacketHelper
{
    /// <summary>
    /// 16진수 문자열을 Byte 배열로 변환
    /// </summary>
    /// <param name="hex">16진수 문자열 (길이는 짝수여야 함)</param>
    /// <returns>byte[]</returns>
    /// <exception cref="ArgumentException"></exception>
    public static byte[] HexStringToByteArray(string hex)
    {
        hex = hex.Replace(" ", "").ToUpper(); // 공백 제거 및 대문자화
        if (hex.Length % 2 != 0)
            throw new ArgumentException("16진수 문자열의 길이는 짝수여야 합니다.");

        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
    }

    /// <summary>
    /// 엔디안에 맞게 바이트 배열(1,2,4바이트)을 정수형 길이로 변환
    /// </summary>
    /// <param name="lengthBytes">byte[] 변수: 길이는 1,2,4바이트 중 하나만 가능</param>
    /// <param name="isLittleEndian">byte[] 변수에 들어 있는 값이 리틀엔디안이면 true, 빅엔디안이면 false</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static int GetLengthFromBytes(byte[] lengthBytes, bool isLittleEndian)
    {
        // 시스템 엔디안과 요청된 엔디안이 다르면 배열 뒤집기
        if (BitConverter.IsLittleEndian != isLittleEndian)
        {
            Array.Reverse(lengthBytes);
        }

        // 길이에 따라 적절한 변환 (1바이트, 2바이트, 4바이트)
        if (lengthBytes.Length == 1) return lengthBytes[0];
        if (lengthBytes.Length == 2) return BitConverter.ToUInt16(lengthBytes, 0);
        if (lengthBytes.Length == 4) return BitConverter.ToInt32(lengthBytes, 0);

        throw new NotSupportedException("지원하지 않는 길이(범위) 단위입니다.");
    }
}
