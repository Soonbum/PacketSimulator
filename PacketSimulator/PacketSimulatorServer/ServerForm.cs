using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using HelperLibrary;

namespace PacketSimulatorServer;

// ArrayPool에서 패킷을 사용하기 좋게 가공한 형태로 채널에 넣게 됨 (ArrayPool 사용시 메모리 부하가 많이 경감됨)
public struct RentedPacket
{
    public byte[] Data; // 풀에서 빌려온 배열
    public int Length;  // 실제 유효한 패킷 길이
}

public partial class ServerForm : Form
{
    private TcpListener _listener;
    private CancellationTokenSource _cts;
    private Channel<RentedPacket> _packetChannel;

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
        _packetChannel = Channel.CreateBounded<RentedPacket>(options);
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
        // 채널에 들어온 패킷을 소비하여 실질적으로 해독 및 처리하는 함수
        _ = Task.Run(() => ConsumePacketsAsync(_cts.Token));

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                // 리스너가 중지되면 여기서 SocketException(995) 발생
                TcpClient client = await _listener.AcceptTcpClientAsync();

                // 패킷이 들어오면 클라이언트별로 패킷을 합친 후 채널로 전달함 (패킷 조립 및 큐잉 담당)
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

    // 패킷이 들어오면 클라이언트별로 패킷을 합친 후 채널로 전달함 (패킷 조립 및 큐잉 담당)
    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using (client)
        using (NetworkStream stream = client.GetStream())
        {
            // 수신용 버퍼를 Pool에서 대여 (new byte[] 제거)
            byte[] readBuffer = ArrayPool<byte>.Shared.Rent(8192);

            // 클라이언트별 누적 버퍼 (조각난 패킷을 모으는 역할)
            List<byte> accumBuffer = new List<byte>();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(readBuffer, 0, readBuffer.Length, token);
                    if (bytesRead == 0) break; // 연결 종료

                    // 새로 수신된 데이터를 누적 버퍼에 추가
                    for (int i = 0; i < bytesRead; i++)
                    {
                        accumBuffer.Add(readBuffer[i]);
                    }

                    // 누적 버퍼 내에서 완성된 패킷 연속 추출 (뭉친 패킷 해결)
                    while (accumBuffer.Count > 0)
                    {
                        int packetLength = TryParsePacketLength(accumBuffer);

                        if (packetLength == 0)
                        {
                            // 아직 패킷이 다 오지 않음 (조각남), 더 수신하도록 반복문 빠져나감
                            break;
                        }
                        else if (packetLength == -1)
                        {
                            // 쓰레기 데이터 처리: 시작 바이트가 나올 때까지 앞부분을 잘라냄
                            int startIndex = accumBuffer.IndexOf(_startByte);
                            if (startIndex == -1)
                            {
                                accumBuffer.Clear(); // 싹 다 버림
                                break;
                            }
                            else
                            {
                                accumBuffer.RemoveRange(0, startIndex);
                                continue; // 잘라내고 다시 검사
                            }
                        }

                        // 완성된 패킷 추출용 버퍼를 Pool에서 대여 (new byte[] 제거)
                        byte[] packetData = ArrayPool<byte>.Shared.Rent(packetLength);
                        accumBuffer.CopyTo(0, packetData, 0, packetLength);

                        // 버퍼에서 처리된 패킷만큼 삭제
                        accumBuffer.RemoveRange(0, packetLength);

                        // 채널로 전송
                        await _packetChannel.Writer.WriteAsync(new RentedPacket { Data = packetData, Length = packetLength }, token);
                    }
                }
            }
            catch
            {
                // 통신 에러 시 루프 종료
            }
            finally
            {
                // 사용이 끝난 수신 버퍼는 반드시 Pool에 반환하여 재사용
                ArrayPool<byte>.Shared.Return(readBuffer);
            }
        }
    }

    /// <summary>
    /// 패킷 내 길이 정보를 추출해서 패킷 길이를 알아내는 함수
    /// </summary>
    /// <param name="buffer">바이트배열 패킷</param>
    /// <returns>패킷 길이</returns>
    private int TryParsePacketLength(List<byte> buffer)
    {
        // 시작 바이트 확인
        int startIndex = buffer.IndexOf(_startByte);
        if (startIndex == -1) return -1; // 시작 바이트 없음

        // 시작 바이트가 맨 앞이 아니면, 그 앞은 쓰레기 데이터이므로 자르도록 -1 반환
        if (startIndex > 0) return -1;

        // 패킷 길이를 읽기 위해 필요한 데이터(오프셋 + 읽기범위)가 수신되었는지 확인
        int lengthIndex = startIndex + _offset;
        if (lengthIndex + _readRange > buffer.Count)
            return 0; // 아직 덜 옴

        // 길이 데이터 추출 (단기 사용용 작은 배열이므로 오버헤드 미미함)
        byte[] lengthBytes = new byte[_readRange];
        buffer.CopyTo(lengthIndex, lengthBytes, 0, _readRange);

        int packetLength = PacketHelper.GetLengthFromBytes(lengthBytes, _isLittleEndian);

        if (packetLength <= 0) return -1; // 잘못된 길이 값 (무한루프 방지)

        // 전체 패킷 길이만큼 수신되었는지 확인
        if (buffer.Count < packetLength)
            return 0; // 덜 옴

        return packetLength;
    }

    // 채널에 들어온 패킷을 소비하여 실질적으로 해독 및 처리하는 함수
    private async Task ConsumePacketsAsync(CancellationToken token)
    {
        try
        {
            await foreach (RentedPacket packet in _packetChannel.Reader.ReadAllAsync(token))
            {
                // UI에 안전하게 넘기기 위해 유효한 길이(Length)만큼만 정확히 잘라냄
                // UI 스레드에서는 컨트롤(TextBox)을 생성해야 하므로 여기서 최종 1회 복사
                byte[] finalPacket = new byte[packet.Length];
                Array.Copy(packet.Data, finalPacket, packet.Length);

                this.Invoke(new Action(() =>
                {
                    RenderPacketToTextBoxes(finalPacket);
                    AddPacketToListBox(finalPacket);
                    UpdateQueueUI();
                }));

                // 중요: 사용이 끝난 대여 배열을 풀에 반납
                ArrayPool<byte>.Shared.Return(packet.Data);

                //await Task.Delay(1000, token);
            }
        }
        catch (OperationCanceledException)
        {
            // 정상 종료
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

    // 대기 중인 패킷을 알려주는 라벨 업데이트
    private void UpdateQueueUI()
    {
        // Channel.Reader.Count 로 현재 큐에 쌓인 대기열 개수를 확인할 수 있습니다 (선택 사항)
        if (_packetChannel.Reader.CanCount)
        {
            lblQueueCount.Text = $"대기 중인 패킷: {_packetChannel.Reader.Count} 개";
        }
    }

    // hex값이 들어있는 바이트배열 패킷을 보기 좋게 나눠서 리스트박스에 넣음
    private void AddPacketToListBox(byte[] packet)
    {
        // byte 배열을 "D1-00-A9" 형태로 변환한 뒤, 하이픈을 공백으로 치환하여 "D1 00 A9"로 만듭니다.
        string hexString = BitConverter.ToString(packet).Replace("-", " ");

        // ListBox에 문자열 추가
        lstPacketHistory.Items.Add(hexString);

        // 항상 가장 최근에 추가된(맨 아래) 항목으로 스크롤이 이동하도록 설정 (선택 사항)
        lstPacketHistory.SelectedIndex = lstPacketHistory.Items.Count - 1;
        lstPacketHistory.ClearSelected(); // 파란색 선택 표시 해제
    }
}
