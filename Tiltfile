# -*- mode: Python -*-
# Assumes a local Kubernetes cluster is already running and configured as the current context.

# Tilt always strips .git from the docker_build context, so GitVersion can't run inside the container; compute it on the host instead.
dotnet = str(local(
  'if command -v dotnet >/dev/null 2>&1; then command -v dotnet; elif [ -x "$HOME/.dotnet/dotnet" ]; then printf "%s" "$HOME/.dotnet/dotnet"; else echo "The .NET SDK is required but dotnet was not found." >&2; exit 127; fi',
  quiet=True,
)).strip()
local(dotnet + " tool restore", quiet=True)
build_version = str(local(dotnet + " tool run dotnet-gitversion /showvariable FullSemVer", quiet=True)).strip()

docker_build(
  "ladirchen-app-webhost",
  context=".",
  dockerfile="LadirchenApp.Webhost/Dockerfile",
  build_args={
    "BUILD_VERSION": build_version,
    "ENABLE_VUE_DEVTOOLS": "true",
  },
)

k8s_yaml(["k8s/deployment.yaml", "k8s/service.yaml"])

k8s_resource(
  "ladirchen-app-webhost",
  port_forwards="3000:8080",
)
