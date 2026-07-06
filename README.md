# PacketSimulator

* 설명
  - byte[] 패킷을 송수신하고 수신측에서 시각적으로 확인할 수 있도록 만든 시뮬레이터
  - .NET 고성능 비동기 큐 Channel을 적용하였음 (버퍼 오버플로우로 인한 메모리 폭발, 서버 다운 방식)
  - ArrayPool 메모리 풀링을 사용하여 메모리 할당 파편화를 방지
  - 패킷이 쪼개져서 들어와도 유실되지 않도록 누적 버퍼 구조 적용

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

* 실제 운영 서버(Production) 구현시 윈도우 서비스 형태로 배포하고 싶다면 "작업자 서비스 (Worker Service)" 템플릿 또는 "ASP.NET Core 웹 API" 템플릿으로 개발하는 것을 추천
  - PacketSimulatorServerWorkerService 프로젝트 참조
    * 작업자 서비스 (Worker Service) 템플릿으로 진행할 경우: 윈도우 서비스만 가능
    * ASP.NET Core 웹 API 템플릿으로 진행할 경우: 윈도우 서비스 + Minimal API를 이용해 웹 서버 구현 가능
  - NuGet 패키지: Microsoft.Extensions.Hosting.WindowsServices 설치 필수
  - 개발이 완료된 후 빌드한 .exe 파일을 윈도우의 sc.exe create 명령어나 PowerShell을 통해 서비스로 등록하면, 완벽한 백그라운드 윈도우 서비스로 동작하게 됨
    * 서비스 등록 방법
      - 배포용 파일 생성: Visual Studio에서 프로젝트를 게시(Publish)하여 실행 파일(.exe)이 포함된 폴더를 준비합니다. (예: "C:\Services\PacketService")
      - __명령 프롬프트(CMD)를 관리자 권한으로 실행합니다.__
      - 서비스 생성 명령어(sc.exe) 실행: 아래 명령어를 입력합니다. (경로와 서비스 이름은 본인의 상황에 맞게 수정하세요)
        ```
        sc.exe create "PacketServer" binPath= "C:\Services\PacketService\PacketSimulatorServerWorkerService.exe" start= auto
        ```
        * "PacketServer": 윈도우 서비스 관리자(services.msc)에 표시될 이름입니다.
        * binPath= "...": 실제 실행 파일이 위치한 절대 경로입니다.
        * start= auto: 윈도우 부팅 시 자동으로 서비스가 시작되도록 설정합니다.
        * __sc 명령어의 문법상 반드시 등호(=) 뒤에 공백(띄어쓰기)이 하나 있어야 합니다.__
    * 서비스 시작
      ```
      sc.exe start "PacketServer"
      ```
      - services.msc를 실행하여 서비스 목록에 등록되었는지 확인합니다.
      - 이후 서비스가 죽거나 재부팅되어도 OS가 자동으로 서버를 실행해 줍니다.
    * 서비스 중지
      ```
      sc.exe stop "PacketServer"
      ```
    * 서비스 삭제
      ```
      sc.exe delete "PacketServer"
      ```
    * ASP.NET Core Web App을 통해서도 접근할 수 있도록 함
      - http://localhost:5000

