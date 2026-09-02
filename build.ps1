# Cake's GitVersion alias needs a real executable on PATH; `dotnet tool restore` alone doesn't produce one for manifest-only local tools.
$toolsDir = Join-Path $PSScriptRoot ".tools"
if (-not (Test-Path (Join-Path $toolsDir "dotnet-gitversion.exe")) -and -not (Test-Path (Join-Path $toolsDir "dotnet-gitversion"))) {
  dotnet tool install --tool-path $toolsDir GitVersion.Tool --version 6.8.2
}
$env:PATH = "$toolsDir$([System.IO.Path]::PathSeparator)$env:PATH"

dotnet run --project build/Build.csproj -- $args
exit $LASTEXITCODE;
