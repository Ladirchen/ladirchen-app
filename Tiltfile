# -*- mode: Python -*-
# Assumes a local Kubernetes cluster is already running and configured as the current context.

docker_build(
  "ladirchen-app-webhost",
  context=".",
  dockerfile="LadirchenApp.Webhost/Dockerfile",
)

k8s_yaml(["k8s/deployment.yaml", "k8s/service.yaml"])

k8s_resource(
  "ladirchen-app-webhost",
  port_forwards="8080:8080",
)
