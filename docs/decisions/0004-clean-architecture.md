# Decision: Layered projects for Clean Architecture boundaries

Status: Accepted

## Context

ADR 0001 selected one server-rendered MVC application and rejected speculative architecture layers. This task explicitly requires separate Domain and Application projects now, followed by an Infrastructure project in Phase 2. The deployment, UI, and API constraints in ADR 0001 remain unchanged.

Approval source: the explicit Phase 1 task brief, 2026-10-05.

Relevant requirements include the SRS §6 data model, NFR-07 transaction-safe inventory, NFR-08 multi-step transactions, and NFR-13 controllable time.

## Decision

ECMA remains one deployable ASP.NET Core MVC application using server-rendered Razor. It has no SPA and no separate API application. Source code is split into these projects:

| Project | References | Contains |
|---|---|---|
| `ECMA.Domain` | None | Entities, enums, domain rules, domain exceptions |
| `ECMA.Application` | `ECMA.Domain` | Repository interfaces, `IUnitOfWork`, `IClock`, query DTOs and filters |
| `ECMA.Infrastructure` (planned, next task) | `ECMA.Application`, `ECMA.Domain` | EF Core, repository implementations, PostgreSQL, migrations |
| `ECMA` (existing web) | `ECMA.Application`, and `ECMA.Infrastructure` later | MVC composition root |

Only `ECMA.Domain` and `ECMA.Application` are created in this phase. The existing web project remains at the repository root and does not need to reference them yet.

This decision amends ADR 0001 only where it said not to introduce architecture layers. ADR 0001 remains authoritative for a single deployable server-rendered MVC application, no SPA, and no internal API solely for Razor pages.

## Consequences

Domain has no package or project dependencies. Application depends only on Domain and exposes concrete repository boundaries rather than a generic repository. EF Core, Npgsql, migrations, repository implementations, authentication, controllers, and views remain outside this phase.

The project graph makes domain rules independently testable and prevents persistence concerns from entering Domain or Application. It adds project boundaries and interfaces that Phase 2 must implement. Architecture-reference checks and domain tests verify those boundaries.

## Layout amendment — 2026-10-05

A subsequent explicit restructuring task moved the projects into the conventional layout `src/ECMA.Domain`, `src/ECMA.Application`, and `src/ECMA.Web`. The web project is now named `ECMA.Web`, references Application, and remains the single deployable server-rendered MVC application. Infrastructure remains planned and was not created. The repository had no Phase 1 Domain/Application source or test project files at restructuring time, so only minimal empty Domain/Application project files were created and no test project was retained.
