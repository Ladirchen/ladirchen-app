using System;
using System.Reflection;
using System.Text.RegularExpressions;

using LadirchenApp.Webhost.Models;

namespace LadirchenApp.Webhost.Helpers;

/// <summary>
/// Provides helper methods in relation to <see cref="Assembly"/>.
/// </summary>
public static partial class AssemblyHelper
{
  public static AboutInfo GetAboutInfoFromAssembly(Assembly? aAssembly = null)
  {
    Assembly asm = aAssembly ?? GetFallbackAssembly();
    AssemblyName asmName = asm.GetName();
    string? informationalVersion = GetInformationalVersion(asm);
    string[]? versionParts = informationalVersion?.Split('+', 2);
    var source = versionParts is { Length: 2 } ? SourceMetadata().Match(versionParts[1]) : null;

    return new AboutInfo(
      asmName.Name ?? "Unknown Name",
      asmName.Version,
      versionParts?[0] ?? "Unknown SemVer",
      source is { Success: true } ? source.Groups["sha"].Value : "Unknown Sha",
      informationalVersion,
      source?.Groups["branch"] is { Success: true } branch ? branch.Value : null,
      asm.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version,
      asm.GetCustomAttribute<AssemblyTitleAttribute>()?.Title,
      asm.GetCustomAttribute<AssemblyProductAttribute>()?.Product,
      asm.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company,
      asm.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description,
      asm.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration,
      asm.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright
    );
  }

  [GeneratedRegex(@"\A(?:Branch\.(?<branch>.+)\.Sha\.)?(?<sha>[0-9a-fA-F]{40}|[0-9a-fA-F]{64})\z")]
  private static partial Regex SourceMetadata();

  public static AssemblyName GetAssemblyName(Assembly? aAssembly = null)
  {
    Assembly asm = aAssembly ?? GetFallbackAssembly();

    return asm.GetName();
  }

  public static string? GetInformationalVersion(Assembly? aAssembly = null)
  {
    Assembly asm = aAssembly ?? GetFallbackAssembly();

    return asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
  }

  private static Assembly GetFallbackAssembly()
  {

    return Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
  }
}