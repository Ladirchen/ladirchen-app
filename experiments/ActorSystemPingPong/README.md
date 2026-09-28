# Actor System Ping-Pong Experiment

A small, self-contained experiment (not part of the main `LadirchenApp` solution) exploring:

- a minimal, hand-rolled actor system,
- the same idea built on **Akka.NET** instead,
- both variants running as a **console app** and in the **browser** via [Bootsharp](https://github.com/elringus/bootsharp) (Blazor-less .NET-on-WASM interop),
- **[TUnit](https://tunit.dev/)** unit tests for the actor/game logic,
- **[Playwright .NET](https://playwright.dev/dotnet/)** end-to-end tests that drive the real browser pages.

Two actors, `Ping` and `Pong`, volley a message back and forth a fixed number of times ("rallies"), logging
every step. The point of the experiment is the actor-system plumbing and the WASM hosting story, not the
game itself.

## Layout

```
ActorSystemPingPong.slnx
src/
  PingPong.ActorSystem/        Hand-rolled mini actor framework (mailbox, ActorSystem, IActorRef, ...)
  PingPong.Game/                Ping/Pong actors + match orchestrator, built on PingPong.ActorSystem
  PingPong.Console/              Console host for the hand-rolled variant
  PingPong.Browser/              Bootsharp/WASM host for the hand-rolled variant
  PingPong.Game.Akka/             Ping/Pong actors + match orchestrator, built on Akka.NET
  PingPong.Console.Akka/          Console host for the Akka.NET variant
  PingPong.Browser.Akka/          Bootsharp/WASM host for the Akka.NET variant
tests/
  PingPong.ActorSystem.Tests/     TUnit tests for the hand-rolled framework + game logic
  PingPong.Game.Akka.Tests/       TUnit tests for the Akka.NET game logic
  PingPong.Browser.E2ETests/      Playwright tests driving both Bootsharp pages in a real browser
```

## Why two actor systems?

The task started as "build a simple actor system" but it's fair to ask: why not just use Akka.NET, the
de-facto actor framework for .NET? Both variants are kept side by side so you can compare them directly:

- **`PingPong.ActorSystem`** (hand-rolled) is ~150 lines: a `Channel<T>`-backed mailbox pumped by a single
  `await foreach` loop per actor, no thread pool or `Task.Run` involved anywhere. That is what makes it
  trivially portable to browser-wasm's single-threaded runtime with zero special-casing.
- **`PingPong.Game.Akka`** uses real Akka.NET (`ReceiveActor`, `Props`, `IActorRef`, `ActorSystem`) - the
  framework you'd actually reach for in production. It needed a handful of WASM-specific workarounds (see
  below) but otherwise runs unmodified in the console.

Neither is "better" here; the hand-rolled one is a teaching example of what an actor system minimally
needs to be, and the Akka.NET one shows that a full-featured framework can be squeezed into the browser too
with some care.

## Running the console apps

```powershell
dotnet run --project src\PingPong.Console         # hand-rolled variant
dotnet run --project src\PingPong.Console.Akka    # Akka.NET variant
```

Both print the same shape of output: a log line per message plus a final "match finished" line.

## Running the browser apps

Each browser host is a [Bootsharp](https://github.com/elringus/bootsharp) app: a `net10.0`/`browser-wasm`
class library that exports a C# event (`OnLog`) and an async method (`RunMatchAsync`) to JavaScript, paired
with a plain `index.html` that imports the generated ES module.

```powershell
dotnet publish src\PingPong.Browser -c Debug
dotnet publish src\PingPong.Browser.Akka -c Debug
```

`dotnet publish` generates `bin/bootsharp/index.mjs` (plus TypeScript declarations) next to each `.csproj`.
Serve the project directory over HTTP (ES module imports don't work from `file://`) and open `index.html`,
e.g.:

```powershell
npx serve src\PingPong.Browser
npx serve src\PingPong.Browser.Akka
```

Click "Run match" and watch the log fill in exactly like the console output.

### Making Akka.NET work under browser-wasm

Akka.NET's actor dispatch itself (mailboxes, the default dispatcher, `ReceiveActor` message handling) works
fine on WASM's single-threaded runtime out of the box - .NET's WASM `Task`/thread-pool shim happily runs
`Task`-based continuations cooperatively even without real OS threads. Getting there took fixing a few
browser-specific incompatibilities:

1. **`System.Configuration` is unsupported.** `ActorSystem.Create(name)` without an explicit config falls
   back to `ConfigurationFactory.Load()`, which reads `app.config` via `System.Configuration.ConfigurationManager`
   - an API WASM doesn't implement. Fix: always pass `ConfigurationFactory.Default()` (optionally merged
   with overrides) explicitly.
2. **IL trimming strips reflection-loaded types.** Akka resolves its actor-ref provider, dispatchers, and
   loggers by type name from HOCON config via reflection. The WASM linker trims unreferenced members by
   default, breaking that resolution (`'akka.actor.provider' is not a valid type name`). Fix:
   `<TrimmerRootAssembly Include="Akka" />` in `PingPong.Browser.Akka.csproj`, which keeps the whole `Akka`
   assembly untrimmed.
3. **Unsupported OS calls at startup and shutdown**:
   - `StandardOutLogger` tries to set `Console.ForegroundColor` for colorized console messages -
     unsupported under the WASM console. Fixed via HOCON: `akka.stdout-loglevel = off`.
   - `CoordinatedShutdown` registers a POSIX/CLR process-exit signal handler, which throws
     `PlatformNotSupportedException` on WASM. Fixed via HOCON: `akka.coordinated-shutdown.run-by-clr-shutdown-hook = off`.
   - `ActorSystem.Terminate()`'s coordinated-shutdown phase machinery also uses a blocking `Monitor.Wait`
     that WASM's single-threaded runtime cannot support. The browser host therefore reuses one actor system
     for the lifetime of the WASM runtime and each match stops its own ping and pong actors. It does not
     attempt per-match actor-system shutdown, so successful browser runs produce no shutdown warning.
     The console host still terminates its actor system normally after each match.

None of this was needed for the console host, which has real threads and OS APIs available - it's purely a
browser-wasm story.

## Running the tests

### Unit tests (TUnit)

> `dotnet test` doesn't work on the .NET 10 SDK ("Testing with VSTest target is no longer supported").
> Each TUnit test project is its own executable (Microsoft.Testing.Platform runner), so run it directly:

```powershell
dotnet run --project tests\PingPong.ActorSystem.Tests -- --report-trx
dotnet run --project tests\PingPong.Game.Akka.Tests -- --report-trx
```

### End-to-end tests (Playwright .NET)

`PingPong.Browser.E2ETests` publishes both browser apps, serves each from an in-process static file server,
then drives real Chromium via Playwright to click "Run match" and assert on the resulting DOM/log text.

```powershell
dotnet run --project tests\PingPong.Browser.E2ETests
```

(Playwright's browser binaries are installed automatically via a `[Before(TestSession)]` hook the first
time this runs.)

## Namespace gotcha

A class named `ActorSystem`/`AkkaGame` cannot live in a C# namespace that has the same name as one of its
own segments (e.g. a class `ActorSystem` inside namespace `...ActorSystem...`, or `using Akka.Actor;` inside
a namespace ending in `.Akka`) - the compiler treats the namespace segment as shadowing the external
namespace/type (`CS0118`/`CS0234`). Every project touching `Akka.*` or defining its own `ActorSystem` type
in this experiment sets an explicit `<RootNamespace>` to sidestep this (see `PingPong.ActorSystem`'s
`PingPong.Actors` namespace and `PingPong.Game.Akka`'s `PingPong.AkkaGame` namespace).
