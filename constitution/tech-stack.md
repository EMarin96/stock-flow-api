# Tech stack and conventions

## Technologies

- **Language:** C#
- **Framework / runtime:** .NET + ASP.NET Core (Minimal APIs)
- **Database:** PostgreSQL — Entity Framework Core for writes, Dapper for reads
- **Tests:** xUnit
- **Deployment:** Docker containers on Azure

## Key files / modules

Clean Architecture, split into 4 projects (one per layer):

- `src/Domain/` — entities, value objects, and domain logic (Products, StockMovements, Locations); no external dependencies.
- `src/Application/` — use cases organized as Vertical Slices: one folder per feature/entity (e.g. `Products/`), each containing one subfolder per use case (`CreateProduct/`, `GetProductById/`, …). Validators, contracts, and DTOs shared by multiple use cases within the same feature live in a `Shared/` subfolder inside that feature, to avoid duplication.
- `src/Infrastructure/` — data access implementations (EF Core for writes, Dapper for reads) and external integrations.
- `src/Api/` — Minimal API project: endpoint definitions, DI wiring, app startup.

## Commands

- `docker-compose up` — starts the local dev environment (API + PostgreSQL).
- `dotnet test` — runs the full test suite. Integration tests spin up a disposable PostgreSQL container via Testcontainers, so Docker must be running locally, but no manual `docker-compose up` step is needed just for tests.
- `dotnet format` — formats and checks code style per `.editorconfig`, backed by Roslyn/StyleCop analyzers for additional rules.
- `dotnet build` — builds the solution.

## Data / domain model

- `Product` — unique SKU per product; defines a minimum stock threshold that triggers a restock alert. Current stock is **not** a direct field — it's calculated from its `StockMovement` records.
- `StockMovement` — fixed types: `IN` / `OUT` / `TRANSFER` / `ADJUSTMENT`. Stock can never go negative. Movements are immutable (append-only) — corrections are made via an inverse movement, never by editing or deleting an existing one.
- `Location` (warehouse) — stock is calculated per product + location combination, not globally. A `TRANSFER` movement affects two locations at once (subtracts from the source, adds to the destination).
- `User` — each user has exactly one fixed role (`admin` / `operator` / `read-only`) that determines their permissions.

## Conventions

- **Code language:** English throughout — identifiers, comments, and error messages.
- **Naming style:** standard .NET conventions — PascalCase for public types/members, camelCase for locals/parameters, `_camelCase` for private fields.
- **Early return:** prefer early returns / guard clauses over nested conditionals for readability.
- **Descriptive names:** variables, constants, methods, and functions must have names that clearly explain their purpose — avoid ambiguous abbreviations.
- **Tests:** a single test project for the whole solution, organized internally into folders that mirror `src/`.
- **Error handling / validation:** Result pattern for expected/business errors (no exceptions used for control flow), FluentValidation for input validation, and a global exception-handling middleware that translates unhandled failures into `ProblemDetails`.
- **Project-specific patterns to follow:**
  - CQRS — commands and queries are separated; EF Core handles writes, Dapper handles reads.
  - Repository pattern (write-side) + Unit of Work — each write-side aggregate has a repository (e.g. `IProductWriteRepository`) for tracking/lookups; a single `IUnitOfWork.SaveChangesAsync()` centralizes the commit and the translation of database errors (e.g. unique constraint violations), avoiding duplicating that handling in every repository and enabling atomic operations across multiple entities (needed later for `StockMovement` `TRANSFER`).
  - Value Objects for Domain invariants (e.g. `Sku`, `Money`) — self-validate via guard clauses that throw a Domain-level `DomainValidationException`, not the Application-layer `Result<T>` (Domain must not depend on Application). Primitive→VO conversion happens only inside the owning entity; outer layers keep using primitives, except dependency-free enums (e.g. `Currency`), which flow through as typed values.
  - A custom, in-house Mediator implementation (not the MediatR library, due to its paid license).
  - Options Pattern for configuration access (`appsettings`).
  - Structured logging with Serilog.
  - Correlation ID / request tracing.
  - Soft delete — no physical deletes.
  - OpenAPI/Swagger for API documentation.
  - A standard pagination pattern on list endpoints.
  - JWT-based authentication with role-based authorization (admin/operator/read-only).
  - Idempotency keys on write endpoints.
  - When the same `Error` (same code/message template) is used in more than one place within a feature, define it once as a static factory in that feature's `Shared/` folder (e.g. `ProductErrors.cs`) and reuse it, instead of duplicating the `Error.X(...)` call.

## Hard limits

- Never commit secrets or credentials (`.env`, `appsettings.Production.json`, real connection strings, etc.) to the repo.
- Never add a new NuGet dependency without checking with the project owner first (MediatR was already ruled out for this exact reason, due to its license).
- Never bypass the Result pattern with exceptions used for control flow — exceptions are reserved for truly unexpected errors.
- Never modify or delete existing `StockMovement` records — the table is append-only; only insert new records, to preserve traceability and audit history.
