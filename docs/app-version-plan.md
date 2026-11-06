# App version display plan (#55)

## Scope

Implemented: the interactive `/app` header and profile show the backend app version alongside the current-member indicator. The full assembly informational version is available as a tooltip and to assistive technology. Loading and unavailable states are localized, with a retry action on failure.

## Existing source of truth

- `Build.GitVersion.props` configures GitVersion.MsBuild to generate assembly version metadata when enabled. The webhost imports it transitively through `Build.NSwag.props`.
- `LadirchenApp.Webhost/Helpers/AssemblyHelper.cs` already reads the assembly product/informational version and produces `AboutInfo`, including `SemVer`, `Sha`, and `InformationalVersion`.
- `LadirchenApp.Webhost/Extensions/WebApplicationExtensions.cs` already maps `GET /about` through `MapAboutInfo`. Reuse it rather than introducing another version endpoint.
- Do not use the frontend package version, a frontend build timestamp, or a separately calculated Git version.

## Implementation steps

1. **Ensure assembly provenance.** Retain the existing GitVersion.MsBuild integration. Inspect a Release build's `AssemblyInformationalVersionAttribute` and confirm that `/about` returns that same GitVersion-generated informational version. Pass the webhost assembly explicitly to the helper so test-host entry assemblies cannot change the reported application version. Preserve the existing response fields.
2. **Add a typed frontend boundary.** Introduce a small application-info port and an HTTP adapter wired in `src/app/composition-root.ts`. Request `${apiBaseUrl}/about` using the existing runtime API base URL. Validate the response before exposing `semVer` and `informationalVersion`; reject missing/placeholder version metadata instead of presenting it as a valid version.
3. **Display in the app and profile.** Use the shared `ApplicationVersion` component in the `/app` header and profile, showing `semVer` and the full `informationalVersion` as accessible secondary detail. Allow the session row to wrap on narrow screens without overlapping content.
4. **Handle loading, failures, and freshness.** Fetch on profile entry with `cache: "no-store"` and ensure the endpoint is not service-worker cached. Add `/about` to the PWA navigation-fallback denylist so direct navigation cannot return the SPA instead of metadata. Show localized loading and unavailable states with a retry action; report failures through the existing notification/error mechanism. Do not block sign-in, session restoration, or family hydration when version information is unavailable. Reopening the profile should refresh the backend version after a deployment.
5. **Verify end to end.** Add backend tests comparing the endpoint's metadata with the webhost assembly, adapter tests for valid/invalid/non-2xx responses, and UI tests for loading, success, failure, and retry. Test both German and English, narrow screens, and a deployment where frontend assets are older than the backend.

## Acceptance criteria

- The displayed version comes only from the running backend's GitVersion-generated .NET assembly metadata.
- A Release build with GitVersion enabled returns the expected semantic and full informational versions through `/about`.
- Guardians, children, and external guardians can find the version in their profile.
- An unavailable endpoint never produces an invented version or prevents normal app use.
- Required validation: frontend `vp check`, `vp test run`, `vp run type-check`, project localization/style checks, and targeted backend tests.
