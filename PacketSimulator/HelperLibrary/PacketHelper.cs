namespace HelperLibrary;

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
