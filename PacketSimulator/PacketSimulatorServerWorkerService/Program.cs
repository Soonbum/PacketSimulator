using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PacketSimulatorServer;
using PacketSimulatorServerWorkerService;
using Serilog;

// 1. 실행 파일(.exe)이 위치한 실제 절대 경로를 가져옵니다.
// 예: C:\Services\PacketService\
string basePath = AppContext.BaseDirectory;

// 2. 절대 경로에 기반한 로그 파일 경로를 안전하게 조합합니다.
string logFilePath = Path.Combine(basePath, "logs", "packet-log-.txt");

// 3. 조합된 절대 경로를 Serilog에 넘겨줍니다.
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        path: logFilePath,
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    // .NET 7/8 최신 빌더 패턴
    var builder = WebApplication.CreateBuilder(args);

    // 기본 로거 대신 Serilog를 사용하도록 설정
    builder.Services.AddSerilog();

    // 윈도우 서비스 전용 설정 (위에서 설치한 패키지가 이 기능을 제공합니다)
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "PacketServerSimulator"; // 서비스 목록(services.msc)에 표시될 이름
    });

    // PacketStore를 싱글톤(1개만 생성)으로 등록하여 Worker와 Web이 공유하게 합니다.
    builder.Services.AddSingleton<PacketStore>();

    // 설정 파일(appsettings.json) 바인딩
    builder.Services.Configure<PacketServerSettings>(
        builder.Configuration.GetSection("PacketServer"));

    // Worker 등록
    builder.Services.AddHostedService<Worker>();

    // 웹 서버 포트 지정 (예: 5000번)
    builder.WebHost.UseUrls("http://*:5000");

    var app = builder.Build();

    // ==========================================
    // 3. 웹 API 및 대시보드 라우팅 설정
    // ==========================================

    // API 엔드포인트: 패킷 데이터를 JSON으로 반환
    app.MapGet("/api/packets", (PacketStore store) =>
    {
        return store.GetRecentPackets();
    });

    // 메인 화면 (초간단 HTML 대시보드 내장)
    app.MapGet("/", () =>
    {
        string html = @"
                <html>
                <head>
                    <title>패킷 모니터링</title>
                    <style>
                        body { background: #1e1e1e; color: #d4d4d4; font-family: monospace; padding: 20px; }
                        h2 { color: #569cd6; }
                        #logBox { background: #000; padding: 15px; border: 1px solid #333; height: 500px; overflow-y: auto; }
                        .packet { margin: 5px 0; border-bottom: 1px dashed #333; padding-bottom: 5px; }
                    </style>
                </head>
                <body>
                    <h2>실시간 패킷 모니터링 대시보드</h2>
                    <div id='logBox'>로딩 중...</div>
                    <script>
                        // 1초마다 API를 찔러서 최신 패킷을 가져와 화면에 그립니다.
                        setInterval(async () => {
                            const response = await fetch('/api/packets');
                            const packets = await response.json();
                            const logBox = document.getElementById('logBox');
                            logBox.innerHTML = packets.map(p => `<div class='packet'>${p}</div>`).join('');
                        }, 1000);
                    </script>
                </body>
                </html>";

        return Results.Content(html, "text/html; charset=utf-8");
    });

    Log.Information("패킷 서버 서비스 시작 (웹 대시보드: http://localhost:5000)");
    app.Run();
}
catch (Exception ex)
{
    // 치명적인 오류로 켜지자마자 죽는 경우를 추적하기 위한 최후의 보루
    Log.Fatal(ex, "호스트가 예기치 않게 종료되었습니다.");
}
finally
{
    // 프로그램 종료 시 남아있는 로그를 완전히 기록
    Log.CloseAndFlush();
}