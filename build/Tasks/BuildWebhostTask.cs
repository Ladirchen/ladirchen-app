using System.Linq;

using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Solution;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Frosting;

namespace Build.Tasks;

public class BuildWebhostTask : FrostingTask<BuildContext>
{
  public override void Run(BuildContext context)
  {
    SolutionParser parser = new(context.FileSystem, context.Environment);
    SolutionParserResult solution = parser.Parse(context.SolutionDirectory.CombineWithFilePath("LadirchenApp.slnx"));

    SolutionProject webhostProj = solution.Projects.First(p => p.Name.EndsWith("Webhost"));
    context.Information(
      "*** Publishing Webhost at {0} to {1}...",
      webhostProj.Path.FullPath,
      context.ArtifactsWebhostDirectory.FullPath
    );

    context.EnsureDirectoryDoesNotExist(context.ArtifactsWebhostDirectory);
    context.EnsureDirectoryExists(context.ArtifactsWebhostDirectory);
    context.DotNetPublish(
      webhostProj.Path.FullPath,
      new DotNetPublishSettings { OutputDirectory = context.ArtifactsWebhostDirectory }
    );
  }
}
