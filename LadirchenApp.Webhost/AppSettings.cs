using System.Collections.Generic;
using Mumrich.SpaDevMiddleware.Domain.Contracts;
using Mumrich.SpaDevMiddleware.Domain.Models;

namespace LadirchenApp.Webhost;

public class AppSettings : ISpaMiddlewareSettings
{
  public Dictionary<string, SpaSettings> SinglePageApps { get; set; } = new();
  public string BasePublicPath { get; set; } = string.Empty;
}
