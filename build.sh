# Cake's GitVersion alias needs a real executable on PATH; `dotnet tool restore` alone doesn't produce one for manifest-only local tools.
tools_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/.tools"
if [ ! -f "$tools_dir/dotnet-gitversion" ]; then
  dotnet tool install --tool-path "$tools_dir" GitVersion.Tool --version 6.8.2
fi
export PATH="$tools_dir:$PATH"

dotnet run --project ./build/Build.csproj -- "$@"
