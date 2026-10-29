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

The web host stores authentication accounts and normalized, typed family aggregate state in
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

## Vercel beta deployment

The root `Dockerfile` publishes the Vue application and ASP.NET host into one
container. The browser therefore uses the same origin for static assets, API
requests, the authentication cookie and SignalR. `vercel.json` deploys the HTTP
application with the Container preset and keeps one production daemon running
in `fra1` for durable outbox delivery and weekly interest settlement.

Configure these Production environment variables in the Vercel project:

| Variable                               | Required | Purpose                                                                                                  |
| -------------------------------------- | -------- | -------------------------------------------------------------------------------------------------------- |
| `DATABASE_URL` or `POSTGRES_URL`       | Yes      | Pooled PostgreSQL connection for accounts, aggregates, outbox messages and Data Protection keys.         |
| `REDIS_URL`                            | Yes      | Shared SignalR backplane. Use a TLS `rediss://` URL in production.                                       |
| `PORT`                                 | Yes      | HTTP port for the web container; use `8080` to match the image default.                                  |
| `DATA_PROTECTION_CERTIFICATE_BASE64`   | Yes      | Base64-encoded PKCS#12 certificate with its private key, used to encrypt persisted Data Protection keys. |
| `DATA_PROTECTION_CERTIFICATE_PASSWORD` | Yes      | Password of the PKCS#12 certificate. Store it as a Vercel secret.                                        |

Keep the same Data Protection certificate available across deployments and
instances. Replacing it without a controlled key migration makes existing key
ring entries unreadable and invalidates authentication cookies. The daemon's
internal health endpoint uses the fixed port `8081`, matching `vercel.json`.

Place PostgreSQL and Redis in, or as close as possible to, Vercel region
`fra1`. If another data region is selected, update the daemon instance region
in `vercel.json` before deployment. For the beta, both web and daemon startup
apply pending database migrations; PostgreSQL serializes concurrent migration
attempts. The separate `--migrate` process role is available when deployment
migrations move to an explicit release job. The daemon exposes `/health/live`
only inside its container, while the scale-to-zero web process does not run
background workers.

The daemon feature is production-only. Preview deployments can exercise the
HTTP application but do not settle interest or dispatch outbox messages. SignalR
uses WebSockets directly and Redis distributes notifications across web
instances. Clients reconnect and reload their read models after an interrupted
connection. Preview deployments need their own `DATABASE_URL` or `POSTGRES_URL`;
configure Redis and the Data Protection certificate there as well when a
production-like preview is required. During daemon shutdown, new settlement and
outbox cycles stop immediately while current operations receive up to 50 seconds
to finish inside the 60-second platform grace period.

Deploy from the repository root with the Vercel framework preset set to
**Container**. The checked-in `vercel.json` selects the preset as well, so no
custom build command, output directory or route rewrite is required. See the
[Vercel Container runtime](https://vercel.com/docs/functions/runtimes/container),
[Vercel Daemons](https://vercel.com/docs/daemons) and
[WebSocket support](https://vercel.com/changelog/websocket-support-is-now-generally-available)
documentation for the platform lifecycle and limits.

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
