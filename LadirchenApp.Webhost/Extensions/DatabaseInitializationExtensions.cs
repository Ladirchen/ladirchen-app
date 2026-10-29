using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure.Persistence;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LadirchenApp.Webhost.Extensions;

public static class DatabaseInitializationExtensions
{
  public static async Task MigrateLadirchenDatabaseAsync(
    this WebApplication application,
    CancellationToken cancellationToken = default
  )
  {
    await using var scope = application.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<LadirchenDbContext>();
    await database.Database.MigrateAsync(cancellationToken);
  }
}
