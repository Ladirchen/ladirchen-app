# -*- mode: Python -*-
# Assumes a local Kubernetes cluster is already running and configured as the current context.

# Tilt always strips .git from the docker_build context, so GitVersion can't run inside the container; compute it on the host instead.
build_version = str(local("gitversion /showvariable FullSemVer", quiet=True)).strip()

docker_build(
  "ladirchen-app-webhost",
  context=".",
  dockerfile="LadirchenApp.Webhost/Dockerfile",
  build_args={"BUILD_VERSION": build_version},
)

k8s_yaml(["k8s/deployment.yaml", "k8s/service.yaml"])

k8s_resource(
  "ladirchen-app-webhost",
  port_forwards="8080:8080",
)

