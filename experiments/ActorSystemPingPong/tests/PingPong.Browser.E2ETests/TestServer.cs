using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace PingPong.Browser.E2ETests;

/// <summary>
/// Publishes the Bootsharp browser apps (both the hand-rolled and Akka.NET variants) and serves each
/// over HTTP for the duration of the test session, so Playwright tests can navigate to a real page
/// instead of the file:// protocol (required for ES module imports to work).
/// </summary>
internal static class TestServer
{
    private static readonly Dictionary<string, StaticFileServer> Servers = new();
    private static readonly Dictionary<string, string> BaseUrls = new();

    public static string HandRolledBaseUrl => BaseUrls["PingPong.Browser"];

    public static string AkkaBaseUrl => BaseUrls["PingPong.Browser.Akka"];

    public static async Task StartAsync()
    {
        var solutionRoot = FindSolutionRoot();

        foreach (var projectName in new[] { "PingPong.Browser", "PingPong.Browser.Akka" })
        {
            var projectDir = Path.Combine(solutionRoot, "src", projectName);

            await PublishAsync(projectDir);

            var port = GetFreeTcpPort();
            var server = new StaticFileServer(projectDir, port);
            server.Start();

            Servers[projectName] = server;
            BaseUrls[projectName] = server.BaseUrl;
        }
    }

    public static void Stop()
    {
        foreach (var server in Servers.Values)
            server.Dispose();
    }

    private static async Task PublishAsync(string projectDir)
    {
        var startInfo = new ProcessStartInfo("dotnet", ["publish", projectDir, "-c", "Debug"])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start dotnet publish.");

        // Read both streams concurrently with waiting for exit so the pipe buffers never fill up
        // and deadlock the publish process (which prints a lot of output while compiling wasm).
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"dotnet publish failed with exit code {process.ExitCode}:\n{stdout}\n{stderr}");
    }

    private static int GetFreeTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ActorSystemPingPong.slnx")))
            dir = dir.Parent;

        return dir?.FullName
            ?? throw new InvalidOperationException($"Could not locate solution root from {AppContext.BaseDirectory}");
    }
}
