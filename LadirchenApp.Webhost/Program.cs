using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;

namespace LadirchenApp.Webhost;

public static class Program
{
  private static async Task Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);
    var app = builder.Build();

    app.MapGet("/", () => "Hello World!");

    await app.RunAsync();
  }
}
