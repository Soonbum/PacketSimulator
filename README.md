# PacketSimulator

* 설명
  - byte[] 패킷을 송수신하고 수신측에서 시각적으로 확인할 수 있도록 만든 시뮬레이터
  - .NET 고성능 비동기 큐 Channel을 적용하였음

<img width="372" height="216" alt="image" src="https://github.com/user-attachments/assets/e9e0a26e-3fbd-4fdd-a5e4-003549980cf1" />

* 클라이언트
  - (1) 엔디안: 리틀엔디안이 기본값으로 되어 있음
  - (2) IP/Port: 접속할 Server의 IP/Port

<img width="489" height="495" alt="image" src="https://github.com/user-attachments/assets/d632f81f-194c-45e7-825d-e9701d0cd27b" />

* 서버
  - (1) 엔디안: 리틀엔디안이 기본값으로 되어 있음
  - (2) IP/Port: 접속할 Server의 IP/Port
  - (3) 시작 Byte: 들어오는 패킷 중에서 시작 Byte가 이 Hex 값으로 된 것만 읽음
  - (4) 길이값 나오는 바이트 오프셋: 길이값이 들어 있는 바이트가 패킷의 몇 번째인지 추출하기 위한 위치 [시작 Byte의 오프셋이 0]
  - (5) 길이값 사이즈: (4)에서 언급한 오프셋으로부터 길이값이 차지하는 필드가 몇 바이트인지 의미함 [8비트(1바이트)/16비트(2바이트)/32비트(4바이트)]
  - (6) 패킷 Reading 체크박스: On 상태이면 패킷 수신 대기, Off 상태이면 패킷 수신 중단
  - (7) 패널 및 여러 개의 텍스트박스: 바이트 단위로 패킷을 쪼개서 보여줌
  - (8) 리스트박스: 처리된 패킷들이 여기에 쌓임
