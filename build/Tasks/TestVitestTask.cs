using Cake.Common;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Build.Tasks;

[TaskName("test-vitest")]
public class TestVitestTask : FrostingTask<BuildContext>
{
  public override void Run(BuildContext context)
  {
    var resultsDirectory = context.ArtifactsRootDirectory.Combine("TestResults/Vitest");
    context.EnsureDirectoryDoesNotExist(resultsDirectory);
    context.EnsureDirectoryExists(resultsDirectory);
    var exitCode = context.StartProcess(
      "vp",
      new ProcessSettings
      {
        Arguments = new ProcessArgumentBuilder()
          .Append("run")
          .Append("ladirchen-app#test")
          .Append("--reporter=default")
          .Append("--reporter=junit")
          .Append("--outputFile.junit")
          .AppendQuoted(resultsDirectory.CombineWithFilePath("vitest.xml").FullPath),
        WorkingDirectory = context.SolutionDirectory,
      }
    );

    if (exitCode != 0)
    {
      throw new CakeException($"Vitest failed with exit code {exitCode}.");
    }
  }
}
