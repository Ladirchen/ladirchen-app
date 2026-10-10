using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Test;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Build.Tasks;

[TaskName("test-domain")]
public class TestDomainTask : FrostingTask<BuildContext>
{
  public override void Run(BuildContext context)
  {
    var resultsDirectory = context.ArtifactsRootDirectory.Combine("TestResults/Domain");
    context.EnsureDirectoryDoesNotExist(resultsDirectory);
    context.EnsureDirectoryExists(resultsDirectory);
    var project = context.SolutionDirectory.CombineWithFilePath(
      "LadirchenApp.Domain.Tests/LadirchenApp.Domain.Tests.csproj"
    );
    context.DotNetTest(
      project.FullPath,
      new DotNetTestSettings
      {
        Configuration = "Release",
        PathType = DotNetTestPathType.Project,
        WorkingDirectory = context.SolutionDirectory,
        ResultsDirectory = resultsDirectory,
        ArgumentCustomization = arguments => arguments
          .Append("--report-trx")
          .Append("--report-trx-filename")
          .AppendQuoted("domain.trx")
          .Append("--report-html-filename")
          .AppendQuoted(resultsDirectory.CombineWithFilePath("domain.html").FullPath),
      }
    );
  }
}
