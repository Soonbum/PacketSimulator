using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    var builder = Host.CreateApplicationBuilder(args);

    // 기본 로거 대신 Serilog를 사용하도록 설정
    builder.Services.AddSerilog();

    // 윈도우 서비스 전용 설정 (위에서 설치한 패키지가 이 기능을 제공합니다)
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "PacketServerSimulator"; // 서비스 목록(services.msc)에 표시될 이름
    });

    // 설정 파일(appsettings.json) 바인딩
    builder.Services.Configure<PacketServerSettings>(
        builder.Configuration.GetSection("PacketServer"));

    // Worker 등록
    builder.Services.AddHostedService<Worker>();

    var host = builder.Build();

    // 서비스 구동 시작 전, 로그가 잘 연결되었는지 첫 메시지를 남겨봅니다.
    Log.Information("패킷 서버 서비스 시작 준비 완료. (경로: {BasePath})", basePath);

    host.Run();
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