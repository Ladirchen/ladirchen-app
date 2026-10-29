using LadirchenApp.Webhost.Infrastructure;
using LadirchenApp.Webhost.Infrastructure.Persistence;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LadirchenApp.Webhost.Extensions;

public static class ServiceCollectionExtensions
{
  private const string DataProtectionApplicationName = "LadirchenApp";

  public static IServiceCollection AddLadirchenDataProtection(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    var dataProtection = services
      .AddDataProtection()
      .SetApplicationName(DataProtectionApplicationName)
      .PersistKeysToDbContext<LadirchenDbContext>();
    var certificate = DataProtectionCertificate.Load(configuration);

    if (certificate is not null)
    {
      services.AddSingleton(certificate);
      dataProtection.ProtectKeysWithCertificate(certificate);
    }

    return services;
  }
}
