using System;

namespace LadirchenApp.Webhost.Models;

public record AboutInfo(string Name, Version? Version, string SemVer, string Sha, string? InformationalVersion);
