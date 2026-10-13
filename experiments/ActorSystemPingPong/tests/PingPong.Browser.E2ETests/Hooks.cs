using System.Diagnostics;

namespace PingPong.Browser.E2ETests;

public class Hooks
{
    [Before(TestSession)]
    public static void InstallPlaywright()
    {
        if (Debugger.IsAttached)
        {
            Environment.SetEnvironmentVariable("PWDEBUG", "1");
        }

        Microsoft.Playwright.Program.Main(["install"]);
    }

    [Before(TestSession)]
    public static Task StartServer() => TestServer.StartAsync();

    [After(TestSession)]
    public static void StopServer() => TestServer.Stop();
}
