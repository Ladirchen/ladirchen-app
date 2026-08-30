# Ladirchen App

Ladirchen ist eine gamifizierte Familienplattform, die Kinder und Jugendliche dabei unterstützt, Verantwortung im Familienalltag zu übernehmen, eigene Ziele zu verfolgen und den bewussten Umgang mit virtuellem Geld zu lernen.

## [Beschreibung & Vision](docs/Ladirchen.md)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [VitePlus](https://viteplus.dev/)
- [Tilt](https://tilt.dev/)
- [GitVersion](https://gitversion.net/)
  - <https://gitversion.net/docs/usage/cli/installation>
- A _local_ Kubernetes cluster
  - [Docker Desktop with Kubernetes](https://www.docker.com/products/docker-desktop)
  - [Rancher Desktop](https://rancherdesktop.io/)
  - [Kind](https://kind.sigs.k8s.io/)
  - [Minikube](https://minikube.sigs.k8s.io/docs/)

## Run

To run the Ladirchen App locally, follow these steps:

1. Ensure all prerequisites are installed and your local Kubernetes cluster is running.
1. Start the development environment using Tilt:

   ```sh
   tilt up
   ```

1. Access the app in your browser at the port specified by your Tilt configuration.

## Build

To build the Ladirchen App, follow these steps:

1. Ensure all prerequisites are installed and your local Kubernetes cluster is running.
1. Navigate to the project root directory.
1. Run the build script:

   **bash**

   ```bash
   ./build.sh -t BuildWebhost
   ```

   **powershell**

   ```powershell
   .\build.ps1 -t BuildWebhost
   ```
