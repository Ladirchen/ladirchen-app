using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Build;

public class BuildContext : FrostingContext
{
  public DirectoryPath SolutionDirectory { get; }
  public DirectoryPath ArtifactsRootDirectory { get; }
  public DirectoryPath ArtifactsWebhostDirectory { get; }

  public BuildContext(ICakeContext context)
    : base(context)
  {
    SolutionDirectory = new DirectoryPath(
      context.Arguments.GetArgument(nameof(SolutionDirectory)) ?? $"{System.IO.Directory.GetCurrentDirectory()}/.."
    );
    ArtifactsRootDirectory = new DirectoryPath(
      context.Arguments.GetArgument(nameof(ArtifactsRootDirectory)) ?? SolutionDirectory.Combine(".artifacts").FullPath
    );
    ArtifactsWebhostDirectory = ArtifactsRootDirectory.Combine("Webhost");

    context.Information("*** Artifacts Root Directory: {0}", ArtifactsRootDirectory.FullPath);
    context.Information("*** Solution Directory: {0}", SolutionDirectory.FullPath);
    context.Information("*** Artifacts Webhost Directory: {0}", ArtifactsWebhostDirectory.FullPath);
  }
}
