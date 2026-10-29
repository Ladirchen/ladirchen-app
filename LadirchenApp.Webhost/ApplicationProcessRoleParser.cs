using System;
using System.Collections.Generic;
using System.Linq;

namespace LadirchenApp.Webhost;

public static class ApplicationProcessRoleParser
{
  public const string DaemonArgument = "--daemon";
  public const string MigrationArgument = "--migrate";

  public static ApplicationProcessRole Parse(IReadOnlyCollection<string> arguments)
  {
    ArgumentNullException.ThrowIfNull(arguments);
    var daemonRequested = arguments.Contains(DaemonArgument);
    var migrationRequested = arguments.Contains(MigrationArgument);

    if (daemonRequested && migrationRequested)
    {
      throw new ArgumentException("The daemon and migration process roles cannot be combined.", nameof(arguments));
    }

    if (daemonRequested)
    {
      return ApplicationProcessRole.Daemon;
    }

    return migrationRequested
      ? ApplicationProcessRole.Migration
      : ApplicationProcessRole.Web;
  }

  public static string[] RemoveRoleArguments(IReadOnlyCollection<string> arguments)
  {
    ArgumentNullException.ThrowIfNull(arguments);
    var hostArguments = new List<string>(arguments.Count);

    foreach (var argument in arguments)
    {
      if (argument is not (DaemonArgument or MigrationArgument))
      {
        hostArguments.Add(argument);
      }
    }

    return hostArguments.ToArray();
  }
}
