# Architecture overview

## Observed baseline

The solution contains `src/ECMA.Web/ECMA.Web.csproj`, `src/ECMA.Application/ECMA.Application.csproj`, and `src/ECMA.Domain/ECMA.Domain.csproj`. Common `net10.0`, nullability, and implicit-using settings are in `Directory.Build.props`; there are no PackageReferences or SDK pin. Domain and Application contain no source files because the referenced Phase 1 files were absent during restructuring.

Current request flow is Browser → ASP.NET Core MVC → `HomeController` → Razor Views. `Index`, `Privacy`, and `Error` are template pages, not product features.

`src/ECMA.Web/Program.cs` registers `AddControllersWithViews`, builds the app, configures production exception handling at `/Home/Error` and HSTS, HTTPS redirection, routing, authorization middleware, `MapStaticAssets`, and conventional `{controller=Home}/{action=Index}/{id?}` routing with `.WithStaticAssets()`. Preserve these modern .NET 10 static-asset conventions.

## Required direction, not yet implemented

Browser → MVC Controllers → business services when needed → EF Core DbContext → PostgreSQL. Controllers return server-rendered Razor Views; a separate frontend/API application is not required. [Decision 0001](../decisions/0001-server-rendered-mvc.md) resolves the SRS architecture conflict in favor of the explicit project brief.

Add services only for meaningful business logic. Web depends on Application, and Application depends on Domain. Infrastructure remains future work per [decision 0004](../decisions/0004-clean-architecture.md). Persistence, accounts, role management, background jobs, mock payment, and all domain features remain unimplemented.

## Cross-cutting concerns

| Concern | Existing implementation | Required direction |
|---|---|---|
| DI | MVC registered in `src/ECMA.Web/Program.cs` | Constructor injection; scoped DbContext/services as appropriate |
| Authentication | None; no authentication registration/middleware | Accepted cookie authentication and database-stored Identity `PasswordHasher<TUser>` hashes; see decision 0002 |
| Authorization | `UseAuthorization`, no protected actions/policies | Single role plus resource ownership/event assignment; immediate role/status changes |
| Validation | Unobtrusive validation partial available; no input forms | ViewModel DataAnnotations, server `ModelState`, service business rules |
| Errors | Non-development exception handler, request ID error ViewModel | Safe errors without stack traces; log unexpected failures |
| Logging | Built-in log-level configuration only | `ILogger<T>`; important operations per NFR-14, no secrets/passwords |
| Time/jobs | None | Testable time source; minute-based idempotent jobs per NFR-11/13 |
| Tests/CI | No test projects, CI, seed/reset scripts | Focused requirement-linked checks; PostgreSQL proof for database concurrency |

The SRS excludes a standalone audit-log feature but requires operational logs and check-in/undo records. Do not confuse these obligations or build a general audit subsystem.

See [product context](../product/agent-context.md) for feature dependencies and unresolved business rules.

Styling direction is Razor with locally compiled Tailwind CSS v4 per [decision 0003](../decisions/0003-razor-tailwind-styling.md). `src/ECMA.Web/package.json` and `src/ECMA.Web/bun.lock` declare the Tailwind CLI packages; the layout still uses Bootstrap and no Tailwind build is configured. These asset dependencies do not change the server-rendered architecture.