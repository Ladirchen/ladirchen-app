using Cake.Common.Diagnostics;
using Cake.Common.Tools.GitVersion;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Build;

public class BuildContext : FrostingContext
{
  public DirectoryPath SolutionDirectory { get; }
  public DirectoryPath ArtifactsRootDirectory { get; }
  public DirectoryPath ArtifactsWebhostPublishDirectory { get; }
  public FilePath ArtifactsWebhostZipFile { get; }
  public FilePath ArtifactsWebhostDockerImageFile { get; }
  public string WebhostDockerImageTag { get; }

  public GitVersion GitVersion { get; }

  public BuildContext(ICakeContext context)
    : base(context)
  {
    SolutionDirectory = new DirectoryPath(
      System.IO.Path.GetFullPath(
        context.Arguments.GetArgument(nameof(SolutionDirectory)) ?? $"{System.IO.Directory.GetCurrentDirectory()}/.."
      )
    );
    ArtifactsRootDirectory = new DirectoryPath(
      context.Arguments.GetArgument(nameof(ArtifactsRootDirectory)) ?? SolutionDirectory.Combine(".artifacts").FullPath
    );
    ArtifactsWebhostPublishDirectory = new DirectoryPath(
      context.Arguments.GetArgument(nameof(ArtifactsWebhostPublishDirectory))
        ?? ArtifactsRootDirectory.Combine("Webhost").FullPath
    );
    GitVersion = context.GitVersion();
    ArtifactsWebhostZipFile = new FilePath(
      context.Arguments.GetArgument(nameof(ArtifactsWebhostZipFile))
        ?? ArtifactsRootDirectory
          .Combine("WebhostZips")
          .CombineWithFilePath($"Webhost-{GitVersion.FullSemVer}.zip")
          .FullPath
    );
    WebhostDockerImageTag =
      context.Arguments.GetArgument(nameof(WebhostDockerImageTag)) ?? $"ladirchen-app-webhost:{GitVersion.SemVer}";
    ArtifactsWebhostDockerImageFile = new FilePath(
      context.Arguments.GetArgument(nameof(ArtifactsWebhostDockerImageFile))
        ?? ArtifactsRootDirectory
          .Combine("WebhostDockerImages")
          .CombineWithFilePath($"Webhost-{GitVersion.FullSemVer}.tar")
          .FullPath
    );
    context.Information("*** Solution Directory: {0}", SolutionDirectory.FullPath);
    context.Information("*** Artifacts Root Directory: {0}", ArtifactsRootDirectory.FullPath);
    context.Information("*** Artifacts Webhost Directory: {0}", ArtifactsWebhostPublishDirectory.FullPath);
    context.Information("*** Artifacts Webhost Zip Directory: {0}", ArtifactsWebhostZipFile.FullPath);
    context.Information("*** Webhost Docker Image Tag: {0}", WebhostDockerImageTag);
    context.Information("*** Artifacts Webhost Docker Image File: {0}", ArtifactsWebhostDockerImageFile.FullPath);
  }
}
