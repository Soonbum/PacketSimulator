using HelperLibrary;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace PacketSimulatorServer;

public partial class ServerForm : Form
{
    private TcpListener _listener;
    private CancellationTokenSource _cts;
    private Channel<byte[]> _packetChannel;

    private byte _startByte;
    private int _offset;
    private int _readRange;
    private bool _isLittleEndian;

    public ServerForm()
    {
        InitializeComponent();

        // BoundedChannel을 사용하여 메모리 폭발 방지 (예: 최대 1만 개 대기)
        // 큐가 가득 차면 가장 오래된 패킷을 버리도록 설정
        var options = new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        };
        _packetChannel = Channel.CreateBounded<byte[]>(options);
    }

    private void chkStart_CheckedChanged(object sender, EventArgs e)
    {
        if (chkStart.Checked)
        {
            // 유효성 검사
            if (!int.TryParse(txtOffset.Text, out int offset) ||
                !int.TryParse(txtReadRange.Text, out int readRange))
            {
                MessageBox.Show("오프셋과 읽기 범위는 숫자여야 합니다.");
                chkStart.Checked = false;
                return;
            }

            StartServer();
        }
        else
        {
            StopServer();
        }
    }

    private async void StartServer()
    {
        // 서버 시작 시점(UI 스레드)에서 UI 컨트롤의 값을 파싱하여 변수에 저장
        try
        {
            // 시작 바이트 파싱 ("D1" 같은 16진수 문자열을 byte로 변환)
            string hexStr = txtStartByte.Text.Trim().ToUpper();
            _startByte = Convert.ToByte(hexStr, 16);

            // 오프셋과 읽기 범위 파싱
            _offset = int.Parse(txtOffset.Text.Trim());
            _readRange = int.Parse(txtReadRange.Text.Trim());

            // 엔디안 모드 캐싱
            _isLittleEndian = rbLittleEndian.Checked;
        }
        catch (Exception ex)
        {
            MessageBox.Show("설정값이 올바르지 않습니다. 다시 확인해주세요.\n" + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            chkStart.Checked = false; // 시작 체크박스 해제
            return; // 파싱 실패 시 서버 시작 취소
        }

        // 정상적으로 서버 구동 및 백그라운드 Task 시작

        int port = int.Parse(txtPort.Text);
        _listener = new TcpListener(IPAddress.Parse(txtIP.Text), port);
        _cts = new CancellationTokenSource();
        _listener.Start();

        // 소비자(Consumer) 태스크 시작: 채널에 데이터가 들어오면 처리
        _ = Task.Run(() => ConsumePacketsAsync(_cts.Token));

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                // 리스너가 중지되면 여기서 SocketException(995) 발생
                TcpClient client = await _listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client, _cts.Token));
            }
        }
        catch (ObjectDisposedException)
        {
            // 리스너가 객체 해제되면서 발생하는 정상 종료 예외
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
        {
            // 오류 코드 995: 사용자가 서버를 중지하여 I/O 작업이 취소된 경우 (정상 종료)
        }
        catch (Exception ex)
        {
            // 진짜 예기치 않은 오류가 발생했을 경우에만 메시지 표시
            if (!_cts.Token.IsCancellationRequested)
            {
                MessageBox.Show($"서버 수신 대기 중 오류가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void StopServer()
    {
        _cts?.Cancel();
        _listener?.Stop();
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using (client)
        using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[8192]; // 대규모 패킷을 고려해 버퍼 크기 증가

            while (!token.IsCancellationRequested)
            {
                try
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0) break; // 클라이언트 연결 종료

                    byte[] receivedData = new byte[bytesRead];
                    Array.Copy(buffer, receivedData, bytesRead);

                    // 채널에 패킷 쓰기 (비동기 병목 없음)
                    await _packetChannel.Writer.WriteAsync(receivedData, token);
                }
                catch
                {
                    break; // 통신 에러 발생 시 루프 종료
                }
            }
        }
    }

    private async Task ConsumePacketsAsync(CancellationToken token)
    {
        try
        {
            // 채널에 데이터가 들어올 때마다 비동기로 꺼내옴
            await foreach (byte[] data in _packetChannel.Reader.ReadAllAsync(token))
            {
                // UI를 차단하지 않고 백그라운드에서 파싱 로직 수행
                byte[] parsedPacket = ParsePacket(data);

                if (parsedPacket != null)
                {
                    // 파싱이 완료된 최종 데이터만 UI 스레드에 던짐
                    this.Invoke(new Action(() =>
                    {
                        RenderPacketToTextBoxes(parsedPacket);
                        UpdateQueueUI();
                    }));

                    // --- 의도적인 1초 딜레이 추가 ---
                    // 취소 토큰(token)을 같이 넘겨주면 서버 종료 시 딜레이 중이더라도 즉각 취소됩니다.
                    await Task.Delay(1000, token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 서버 종료 시 Task.Delay 대기 중이거나 ReadAllAsync 대기 중일 때 발생하는 예외 (정상 종료)
        }
    }

    // 미리 배치된 텍스트박스(예: txtByte1, txtByte2, ...)를 갱신
    private void RenderPacketToTextBoxes(byte[] packet)
    {
        // FlowLayoutPanel 등을 이용해 동적으로 생성하거나, 미리 생성된 TextBox 리스트를 순회합니다.
        // 여기서는 pnlPacketBytes 라는 FlowLayoutPanel이 폼에 있다고 가정합니다.
        pnlPacketBytes.Controls.Clear();

        foreach (byte b in packet)
        {
            TextBox tb = new()
            {
                Text = b.ToString("X2"), // 16진수 대문자 표시 [D1] [00]...
                Width = 30,
                ReadOnly = true,
                TextAlign = HorizontalAlignment.Center,
                Margin = new Padding(2)
            };
            pnlPacketBytes.Controls.Add(tb);
        }
    }

    private byte[] ParsePacket(byte[] data)
    {
        // 1. 시작 바이트 위치 검색
        int startIndex = Array.IndexOf(data, _startByte);

        // 시작 바이트를 찾지 못했으면 유효한 패킷이 아님
        if (startIndex == -1)
            return null;

        // 2. 패킷 길이를 읽기 위해 필요한 최소 데이터가 수신되었는지 확인
        int lengthIndex = startIndex + _offset;
        if (lengthIndex + _readRange > data.Length)
            return null; // 아직 길이 정보까지 수신되지 않음 (패킷 잘림 대기)

        // 3. 길이 데이터 추출
        byte[] lengthBytes = new byte[_readRange];
        Array.Copy(data, lengthIndex, lengthBytes, 0, _readRange);

        // 4. 엔디안 설정에 맞춰 바이트 배열을 정수형 길이로 변환 (미리 만든 Helper 사용)
        int packetLength = PacketHelper.GetLengthFromBytes(lengthBytes, _isLittleEndian);

        // 비정상적인 길이 값 방어 로직 (무한 루프나 메모리 에러 방지)
        if (packetLength <= 0)
            return null;

        // 5. 계산된 총 패킷 길이만큼 전체 데이터가 수신되었는지 확인
        if (startIndex + packetLength > data.Length)
            return null; // 아직 전체 패킷이 수신되지 않음 (패킷 잘림 대기)

        // 6. 시작 바이트부터 총 길이만큼의 완전한 패킷 추출
        byte[] finalPacket = new byte[packetLength];
        Array.Copy(data, startIndex, finalPacket, 0, packetLength);

        return finalPacket;
    }

    private void UpdateQueueUI()
    {
        // Channel.Reader.Count 로 현재 큐에 쌓인 대기열 개수를 확인할 수 있습니다 (선택 사항)
        if (_packetChannel.Reader.CanCount)
        {
            lblQueueCount.Text = $"대기 중인 패킷: {_packetChannel.Reader.Count} 개";
        }
    }
}
