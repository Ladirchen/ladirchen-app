# -*- mode: Python -*-
# Assumes a local Kubernetes cluster is already running and configured as the current context.

# Tilt always strips .git from the docker_build context, so GitVersion can't run inside the container; compute it on the host instead.
# local() spawns cmd.exe on Windows and sh elsewhere, so detect the dotnet path in pure Starlark to stay cross-platform.
if os.name == 'nt':
  dotnet_candidate = os.path.join(os.getenv('USERPROFILE', ''), '.dotnet', 'dotnet.exe')
else:
  dotnet_candidate = os.path.join(os.getenv('HOME', ''), '.dotnet', 'dotnet')
dotnet = dotnet_candidate if os.path.exists(dotnet_candidate) else 'dotnet'
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
