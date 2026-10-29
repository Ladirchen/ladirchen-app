# Ladirchen App

Ladirchen ist eine gamifizierte Familienplattform, die Kinder und Jugendliche dabei unterstützt, Verantwortung im Familienalltag zu übernehmen, eigene Ziele zu verfolgen und den bewussten Umgang mit virtuellem Geld zu lernen.

## [Beschreibung & Vision](docs/Ladirchen.md)

## Domain and Tests

The pure C# core economy implements family membership, contributions, wallets,
saving goals, gifts and shop redemption. It reuses the dependency-free DDD
primitives without a database or web-framework dependency. See the
[domain analysis and model](docs/domain/domain-model.md) for business rules,
aggregate boundaries, deliberate prototype differences and deferred features.

English Gherkin specifications use Reqnroll with the existing TUnit runner.
Only the .NET 10 SDK is needed for domain tests:

```powershell
dotnet test --project LadirchenApp.Domain.Tests/LadirchenApp.Domain.Tests.csproj
```

Run only the Gherkin domain scenarios:

```powershell
dotnet test --project LadirchenApp.Domain.Tests/LadirchenApp.Domain.Tests.csproj --treenode-filter '/*/LadirchenApp.Domain.Tests.Features/*/*'
```

## Cake Test Targets

Run both test suites from the repository root:

```powershell
.\build.ps1 -t test-all
```

Run either suite independently with `test-domain` or `test-vitest`:

```powershell
.\build.ps1 -t test-domain
.\build.ps1 -t test-vitest
```

On Linux or macOS, use `./build.sh -t test-all` (or either individual target).
The domain target uses Cake's `DotNetTest` API with the .NET 10 project selector
and Release configuration. The Vitest target uses Cake's process API to run the
existing Vite+ workspace test script. Test failures fail the Cake target.

Cake writes fresh reports for each suite under `.artifacts/TestResults/`:

- `Domain/domain.trx`: machine-readable .NET test results.
- `Domain/domain.html`: human-readable domain test report.
- `Vitest/vitest.xml`: frontend JUnit test results.

GitHub Actions runs the suites in separate steps so a domain test failure does
not skip frontend tests. Reports are uploaded as the `test-results` artifact
even after failures and retained for 14 days. TRX and JUnit results appear in
the workflow job summary. PR comments and separate check runs are disabled,
so publishing does not require write permissions on fork pull requests.
Failed tests still fail the job and prevent build/release publication.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- PostgreSQL 18 for local application development
- Docker for the PostgreSQL integration tests
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

The web host stores authentication accounts and family aggregate snapshots in
PostgreSQL. For local development, create the `ladirchen` database and user
configured in `LadirchenApp.Webhost/appsettings.json`, or override the connection:

```bash
export ConnectionStrings__Ladirchen='Host=localhost;Port=5432;Database=ladirchen;Username=ladirchen;Password=change-me'
```

For a managed PostgreSQL database, including a provider connected through the
Vercel Marketplace, set `DATABASE_URL` or `POSTGRES_URL`. Both standard Npgsql
connection strings and `postgresql://` URLs are accepted. Prefer the provider's
pooled connection URL for serverless deployments. The web host applies the EF
Core migrations when it starts.

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
   ./build.sh -t build-all
   ```

   **powershell**

   ```powershell
   .\build.ps1 -t build-all
   ```
