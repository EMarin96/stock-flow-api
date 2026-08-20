# 002 · Locations/warehouses — Plan

## Approach

Implement as Vertical Slices under `src/Application/Locations/`, one folder per use case (CreateLocation, UpdateLocation, GetLocationById, GetLocations, DeleteLocation), mirroring the exact shape of `src/Application/Products/`. CQRS: write commands go through EF Core, read queries go through Dapper. Reuses the project's existing shared infrastructure — `Entity`/`AuditableEntity`, `IUnitOfWork`, the custom Mediator, `Result`/`Error` — introduced in feature 001, none of which is Product-specific.

## Implementation

1. `src/Domain/Locations/LocationCode.cs` — Value Object, `sealed record` with `Value` (string). Private constructor; `Create(string value)` guard clauses (empty/whitespace, length > 32) throw `DomainValidationException`, mirroring `Sku.cs`.
2. `src/Domain/Locations/Address.cs` — Value Object, `sealed record` with `Street`, `City`, `Country` (all string). Private constructor; `Create(street, city, country)` guard clauses: all three required (non-empty) together — an `Address` is never partially filled. Max lengths: `Street` 200, `City` 100, `Country` 100.
3. `src/Domain/Locations/Location.cs : AuditableEntity` — `Code` (`LocationCode`), `Name` (string), `Address` (`Address?`, nullable — a `Location` can exist without a physical address on file). `Create(string code, string name, string? street, string? city, string? country)` and `Update(string name, string? street, string? city, string? country)` (code immutable, same convention as `Sku`) — build `LocationCode`/`Address` internally, same primitive-in/VO-inside pattern as `Product`.
4. EF Core `LocationConfiguration` (Infrastructure) — `Code` via `HasConversion(...)`, unique index on `Code`; `Address` as an EF **owned type** (`OwnsOne`), nullable as a whole (EF Core supports optional owned references) with `Street`/`City`/`Country` columns, all nullable; Global Query Filter `!IsDeleted`.
5. New EF Core migration `AddLocationsTable` — **additive**, not a regeneration of `InitialCreate` (unlike the Value-Objects refactor in feature 001, `InitialCreate` is already shipped/merged to `development`, so this must be a normal new migration).
6. Dapper read queries (Infrastructure) for `GetLocationById`/`GetLocations`, filtering `IsDeleted = false` explicitly, reusing `ISqlConnectionFactory`.
7. `src/Application/Locations/Shared/` — `ILocationWriteRepository` (`CodeExistsAsync`, `GetByIdAsync`, `AddAsync`), `ILocationReadRepository`, `LocationDto` (flat/primitive — `Code`, `Name`, `Street?`, `City?`, `Country?` — same convention as `ProductDto`, no VOs on the read side), `LocationMappingExtensions`, `LocationErrors` (`NotFound(id)`, `CodeAlreadyExists(code)`, `ValidationFailed(validationResult)` — same shape as `ProductErrors`).
8. `src/Application/Locations/CreateLocation/` — Command (`Code`, `Name`, `Street?`, `City?`, `Country?`) + Handler (pre-checks `Code` uniqueness before insert, in addition to the DB unique index) + Validator (`Code`/`Name` required, `Street`/`City`/`Country` either all present or all absent — FluentValidation custom rule).
9. `src/Application/Locations/UpdateLocation/` — Command + Handler (404 if not found/soft-deleted); `Code` is not part of the update command (immutable). Same "all or none" address validation as Create.
10. `src/Application/Locations/DeleteLocation/` — Command + Handler; soft delete (`IsDeleted = true`, `DeletedAt = now`); 404 if not found/already deleted.
11. `src/Application/Locations/GetLocationById/` — Query + Handler (Dapper); 404 if not found.
12. `src/Application/Locations/GetLocations/` — Query (`page`, `pageSize`) + Handler (Dapper, offset-based pagination) + Validator, same bounds as `GetProductsValidator`.
13. `src/Api/Endpoints/LocationEndpoints.cs` — Minimal API endpoints (create/update/get-by-id/list/delete), wired to the Mediator, mapped in `Program.cs`; Swagger annotations/response types per endpoint.
14. Tests: unit tests per handler/validator + `LocationCodeTests`/`AddressTests` for the new VOs' guard clauses, and integration tests for the 5 endpoints in `tests/StockFlow.Tests/Api/Locations/LocationEndpointsTests.cs`. Reuses the existing shared `PostgresContainerFixture`/`"Postgres collection"` (one Testcontainer for the whole run) with a new `LocationApiFactory`/`LocationApiFactoryFixture` pair mirroring the `Products` ones, truncating the `Locations` table between tests. Test method names follow the established `MethodUnderTest_Scenario_Expected` convention.

## Decisions

- **`LocationCode` and `Address` as Value Objects** — same rationale as `Sku`/`Money` in feature 001: guard clauses throwing `DomainValidationException` (not `Result<T>`, to keep `Domain` free of an `Application` dependency), conversion from primitives happens only inside `Location`. `Address` is modeled as a Value Object (not three loose strings on `Location`) because its three components are only meaningful together — same reasoning as `Money`'s `Amount`+`Currency`.
- **`Address` is optional on `Location`, but internally all-or-nothing** — a `Location` can be created without any address on file (useful for a warehouse whose exact address isn't known yet), but if any of `Street`/`City`/`Country` is provided, all three are required — enforced by a FluentValidation rule at the Application boundary and by `Address.Create`'s guard clause at the Domain boundary.
- **`Code` uniqueness via DB unique index + pre-check** — identical pattern to `Sku`: the unique index is the final safety net against race conditions; `CreateLocationHandler` pre-checks first to return a clean business validation error instead of a raw DB constraint violation, both funneled through the existing `IUnitOfWork`.
- **New migration, not a regenerated `InitialCreate`** — feature 001's migration is already merged to `development`, so this feature adds a normal, additive `AddLocationsTable` migration.
- **Shared `LocationErrors`, from the start** — feature 001 initially duplicated error construction and only centralized it retroactively; this feature applies the now-documented `tech-stack.md` convention (shared error factory per feature) from day one.

## Risks

- **Dapper reads forgetting the `IsDeleted = false` filter** — same risk as Product; mitigate the same way (shared SQL fragment/constant, a test asserting soft-deleted locations never appear in list/get-by-id).
- **Race condition on `Code` uniqueness** between the pre-check and the insert — mitigated by the DB unique index, translated by `IUnitOfWork` into the same business `Result` as the pre-check.
- **Partial address input** (e.g. `Street` given without `City`/`Country`) silently accepted if the "all or none" validation rule is missed on either the Application (`FluentValidation`) or Domain (`Address.Create`) side — both layers must enforce it, not just one.
- **`CreatedBy`/`UpdatedBy` staying `null` indefinitely** until the Auth feature (backlog) is implemented — same tracked risk as Product.
