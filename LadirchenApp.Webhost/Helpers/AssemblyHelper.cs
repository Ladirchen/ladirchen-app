using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using LadirchenApp.Webhost.Models;

namespace LadirchenApp.Webhost.Helpers;

/// <summary>
/// Provides helper methods in relation to <see cref="Assembly"/>.
/// </summary>
public static class AssemblyHelper
{
  public static AboutInfo GetAboutInfoFromAssembly(Assembly? aAssembly = null)
  {
    Assembly asm = aAssembly ?? GetFallbackAssembly();
    AssemblyName asmName = asm.GetName();
    string? informationalVersion = GetInformationalVersion(aAssembly);
    string semver = informationalVersion?.Split('+').FirstOrDefault() ?? "Unknown SemVer";
    string sha = informationalVersion?.Split("Sha.").LastOrDefault() ?? "Unknown Sha";
    string name = asmName.Name ?? "Unknown Name";

    return new AboutInfo(name, asmName.Version, semver, sha, informationalVersion);
  }

  public static AssemblyName GetAssemblyName(Assembly? aAssembly = null)
  {
    Assembly asm = aAssembly ?? GetFallbackAssembly();

    return asm.GetName();
  }

  public static string? GetInformationalVersion(Assembly? aAssembly = null)
  {
    Assembly asm = aAssembly ?? GetFallbackAssembly();
    string asmFilePath = string.IsNullOrWhiteSpace(asm.Location)
      ? Path.Combine(AppContext.BaseDirectory, asm.GetName().Name + ".dll")
      : asm.Location;

    return FileVersionInfo.GetVersionInfo(asmFilePath).ProductVersion;
  }

  private static Assembly GetFallbackAssembly()
  {
    return Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
  }
}
