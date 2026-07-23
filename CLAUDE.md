# AuthForge — Engineering Conventions

This file documents the standing conventions for this repository. Follow them for every change, not just new features.

## Architecture

Clean Architecture, four layers, dependencies point inward only:

```
Presentation -> Application -> Domain
Infrastructure -> Application -> Domain
```

- **Domain**: entities and repository interfaces only. Zero third-party package references. Never depends on Application, Infrastructure, or Presentation.
- **Application**: use cases (services), DTOs (`Contracts`), and interfaces for anything Infrastructure implements (`ICryptoService`, `IJwtService`, etc.). Depends only on Domain and dependency-free abstraction packages (e.g. `Microsoft.Extensions.Configuration.Abstractions`). Never references concrete infrastructure libraries (Dapper, Npgsql, EF Core, Argon2, JWT libraries).
- **Infrastructure**: implements Application's interfaces — repositories, password hashing, JWT issuing, database access (EF Core for writes, Dapper for reads; see below).
- **Presentation**: ASP.NET Core controllers, DI composition root (`ServiceExtensions`), middleware. The only layer allowed to reference every other layer and wire concrete implementations together.

Each project's `.csproj` should reference only the packages its own code actually uses — no copy-pasting the full dependency list across layers.

## Data access: EF Core for writes, Dapper for reads

This project uses PostgreSQL. Writes (insert/update) go through EF Core (`AppDbContext`), reads go through Dapper against the same connection. Rationale: EF Core's change tracking and migrations make writes and schema evolution safe; Dapper keeps read queries fast and explicit without ORM materialization overhead. Repository implementations combine both behind the same repository interface — the Application layer is unaware of which one is used.

- Table/column names are `snake_case` (idiomatic PostgreSQL), generated via `EFCore.NamingConventions`. Dapper relies on `DefaultTypeMap.MatchNamesWithUnderscores = true` to map `snake_case` columns to PascalCase C# properties without manual aliasing.
- Schema changes go through EF Core migrations (`dotnet ef migrations add ...`), not hand-written SQL scripts.

## SOLID / DRY / KISS

- **Single Responsibility**: a class has one reason to change. Controllers only translate HTTP <-> service calls; services hold business rules; repositories only do data access.
- **Dependency Inversion**: Application and Domain depend on interfaces, never on concrete Infrastructure types.
- **DRY**: don't duplicate logic across layers or repeat the same package reference/config in places that don't need it. Extract shared logic instead of copy-pasting.
- **KISS**: prefer the direct, boring solution. Don't add abstractions, configuration flags, or generalization for requirements that don't exist yet (YAGNI applies too).

## Comments

- Write in **English**, always.
- Default to **no comments**. Code should be readable through naming and structure.
- Only add a comment when it explains a non-obvious **why** — a hidden constraint, a workaround, a subtle invariant. Never explain **what** the code does; that belongs in naming, not prose.
- No commented-out code, no TODO graveyards, no restating the method name in a comment.

## Documentation (method summaries)

C# doesn't have Python-style docstrings, so the idiomatic equivalent is standard XML doc comments (`<summary>`, `<param>`, `<returns>`, `<exception>`), applied with the same spirit as Google-style docstrings:

- One concise, imperative-mood summary line (e.g. "Registers a new user.", not "This method registers a new user.").
- Document every parameter and the return value when they aren't self-evident from the signature.
- Document exceptions that can propagate to the caller (`<exception cref="...">`), when relevant.
- Apply this to every public method (controllers, services, repositories, infrastructure classes). Private helpers only need it if the "why" isn't obvious from the name.

```csharp
/// <summary>
/// Grants a user access to an application, reactivating a previously revoked grant if one exists.
/// </summary>
/// <param name="userId">The central identity of the user being granted access.</param>
/// <param name="applicationId">The application the grant applies to.</param>
/// <param name="roles">The role(s) to assign, e.g. "Admin" or "User".</param>
/// <param name="operationUserId">The identity performing this operation, recorded for auditing.</param>
/// <returns>The id of the created or updated grant.</returns>
public async Task<int> GrantAccessAsync(int userId, int applicationId, string roles, int operationUserId)
```

## Git workflow

- Commit incrementally as work progresses — don't batch unrelated changes into one commit.
- Commit messages and PR titles/descriptions are written in **English**.
- Never open a pull request unless explicitly asked to. When asked, provide the PR link, title, and description — the user opens/manages the PR itself unless told otherwise.
