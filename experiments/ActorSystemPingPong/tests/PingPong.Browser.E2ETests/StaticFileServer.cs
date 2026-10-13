using System.Net;

namespace PingPong.Browser.E2ETests;

/// <summary>Tiny static-file HTTP server used to host the published Bootsharp browser app for Playwright.</summary>
internal sealed class StaticFileServer : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly string _root;
    private readonly CancellationTokenSource _cts = new();

    public string BaseUrl { get; }

    public StaticFileServer(string root, int port)
    {
        _root = root;
        BaseUrl = $"http://localhost:{port}/";
        _listener.Prefixes.Add(BaseUrl);
    }

    public void Start()
    {
        _listener.Start();
        _ = AcceptLoopAsync(_cts.Token);
    }

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (Exception) when (ct.IsCancellationRequested || _cts.IsCancellationRequested)
            {
                break;
            }

            _ = HandleAsync(context);
        }
    }

    private async Task HandleAsync(HttpListenerContext context)
    {
        try
        {
            var path = Uri.UnescapeDataString(context.Request.Url!.AbsolutePath.TrimStart('/'));
            if (string.IsNullOrEmpty(path))
                path = "index.html";

            var filePath = Path.GetFullPath(Path.Combine(_root, path));
            if (!filePath.StartsWith(_root, StringComparison.OrdinalIgnoreCase) || !File.Exists(filePath))
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
                return;
            }

            context.Response.ContentType = GetContentType(filePath);
            var bytes = await File.ReadAllBytesAsync(filePath);
            context.Response.ContentLength64 = bytes.Length;
            await context.Response.OutputStream.WriteAsync(bytes);
            context.Response.Close();
        }
        catch
        {
            try
            {
                context.Response.Close();
            }
            catch
            {
                // Client likely disconnected; nothing to clean up.
            }
        }
    }

    private static string GetContentType(string filePath) => Path.GetExtension(filePath).ToLowerInvariant() switch
    {
        ".html" => "text/html",
        ".mjs" or ".js" => "text/javascript",
        ".wasm" => "application/wasm",
        ".json" => "application/json",
        ".symbols" => "text/plain",
        _ => "application/octet-stream",
    };

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();
        _listener.Close();
        _cts.Dispose();
    }
}
