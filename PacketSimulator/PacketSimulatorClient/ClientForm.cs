using System.Net.Sockets;
using System.Text.RegularExpressions;
using HelperLibrary;

namespace PacketSimulatorClient;

public partial class ClientForm : Form
{
    public ClientForm()
    {
        InitializeComponent();
    }

    private void btnSend_Click(object sender, EventArgs e)
    {
        string ip = txtIP.Text;
        int port = int.Parse(txtPort.Text);
        string hexInput = txtPacket.Text.ToUpper(); // ToUpper 처리

        // 유효성 검사 (16진수 여부 및 짝수 길이)
        if (hexInput.Length % 2 != 0 || !Regex.IsMatch(hexInput, @"\A\b[0-9A-F]+\b\Z"))
        {
            MessageBox.Show("패킷은 올바른 16진수 형식이어야 하며, 2글자 단위로 입력해야 합니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            // 패킷 변환 (문자열 -> Byte[])
            byte[] packetData = PacketHelper.HexStringToByteArray(hexInput);

            // 일시적 연결 및 전송 (using 블록을 사용하여 전송 후 즉시 소켓 닫힘)
            using TcpClient client = new(ip, port);
            using NetworkStream stream = client.GetStream();
            stream.Write(packetData, 0, packetData.Length);

            //MessageBox.Show("전송 완료");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"전송 실패: {ex.Message}");
        }
    }
}
