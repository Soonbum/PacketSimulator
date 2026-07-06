using System.Collections.Concurrent;

namespace PacketSimulatorServer;

public class PacketStore
{
    // 스레드 안전한 큐 사용
    private readonly ConcurrentQueue<string> _packets = new();
    private const int MaxCount = 100; // 화면에 보여줄 최근 패킷 개수 유지

    public void AddPacket(string hexString)
    {
        _packets.Enqueue(hexString);

        // 100개가 넘으면 가장 오래된 것을 버림 (메모리 누수 방지)
        if (_packets.Count > MaxCount)
        {
            _packets.TryDequeue(out _);
        }
    }

    public IEnumerable<string> GetRecentPackets()
    {
        // 큐의 데이터를 배열로 복사해서 최신순(역순)으로 반환
        return _packets.ToArray().Reverse();
    }
}