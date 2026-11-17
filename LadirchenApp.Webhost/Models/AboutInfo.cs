using System;

namespace LadirchenApp.Webhost.Models;

/// <summary>Describes the version and published metadata of the running webhost assembly.</summary>
public sealed record AboutInfo(
  string Name,
  Version? Version,
  string SemVer,
  string Sha,
  string? InformationalVersion,
  string? Branch,
  string? FileVersion,
  string? Title,
  string? Product,
  string? Company,
  string? Description,
  string? Configuration,
  string? Copyright
);
