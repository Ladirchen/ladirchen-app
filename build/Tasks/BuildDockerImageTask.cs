using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Docker;
using Cake.Frosting;

namespace Build.Tasks;

public class BuildDockerImageTask : FrostingTask<BuildContext>
{
  public override void Run(BuildContext context)
  {
    context.Information("*** Building Docker image {0}...", context.WebhostDockerImageTag);
    context.DockerBuild(
      new DockerImageBuildSettings
      {
        File = context.SolutionDirectory.CombineWithFilePath("LadirchenApp.Webhost/Dockerfile").FullPath,
        Tag = [context.WebhostDockerImageTag],
        BuildArg = [$"BUILD_VERSION={context.GitVersion.FullSemVer}"],
      },
      context.SolutionDirectory.FullPath
    );

    context.Information("*** Saving Docker image to {0}...", context.ArtifactsWebhostDockerImageFile.FullPath);
    context.EnsureDirectoryExists(context.ArtifactsWebhostDockerImageFile.GetDirectory());
    context.DockerSave(
      new DockerImageSaveSettings { Output = context.ArtifactsWebhostDockerImageFile.FullPath },
      context.WebhostDockerImageTag
    );
    context.Information("*** Docker image archive created at {0}", context.ArtifactsWebhostDockerImageFile.FullPath);
  }
}
