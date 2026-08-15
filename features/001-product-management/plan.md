# 001 · Product management — Plan

## Approach

Implement as Vertical Slices under `src/Application/Products/`, one folder per use case (CreateProduct, UpdateProduct, GetProductById, GetProducts, DeleteProduct). CQRS: write commands go through EF Core, read queries go through Dapper. Each slice has its own Command/Query, Handler (using the project's custom Mediator), and FluentValidation validator.

## Implementation

1. `src/Domain/Common/Entity.cs` — base class holding `Id` (Guid).
2. `src/Domain/Common/AuditableEntity.cs : Entity` — adds `CreatedAt`/`CreatedBy`, `UpdatedAt`/`UpdatedBy`, `IsDeleted`/`DeletedAt`.
3. `src/Domain/Products/Product.cs : AuditableEntity` — `Sku`, `Name`, `Description`, `UnitOfMeasure`, `Price`, `MinimumStockThreshold`.
4. EF Core `ProductConfiguration` (Infrastructure) — unique index on `Sku`; Global Query Filter `!IsDeleted`.
5. Dapper read queries (Infrastructure) for `GetProductById`/`GetProducts`, explicitly filtering `IsDeleted = false` (Dapper does not see EF's global filter).
6. `src/Application/Products/CreateProduct/` — Command + Handler (pre-checks SKU uniqueness before insert, in addition to the DB unique index) + Validator (price ≥ 0, threshold ≥ 0, required fields).
7. `src/Application/Products/UpdateProduct/` — Command + Handler (404 if not found/soft-deleted); SKU is not part of the update command (immutable).
8. `src/Application/Products/DeleteProduct/` — Command + Handler; sets `IsDeleted = true`, `DeletedAt = now`; 404 if not found/already deleted.
9. `src/Application/Products/GetProductById/` — Query + Handler (Dapper); 404 if not found.
10. `src/Application/Products/GetProducts/` — Query (`page`, `pageSize`) + Handler (Dapper, offset-based pagination) + Validator (page/pageSize bounds).
11. `src/Api/Endpoints/ProductEndpoints.cs` — Minimal API endpoints (create/update/get-by-id/list/delete), wired to the custom Mediator, mapped in `Program.cs`.
12. Swagger annotations/response types for each endpoint, per the OpenAPI convention in `constitution/tech-stack.md`.
13. Tests: unit tests per handler/validator + integration tests for the endpoints, in the single test project mirroring `src/` (per `constitution/tech-stack.md`).

## Decisions

- **Entity/AuditableEntity split** — `Entity` holds only `Id`; `AuditableEntity : Entity` adds the audit fields. Keeps the door open for future non-audited entities; every current domain entity (Product, and later StockMovement/Location/User) inherits from `AuditableEntity`.
- **`CreatedBy`/`UpdatedBy` nullable, left `null` for now** — populated once the Auth feature (backlog) supplies a real user identity. Discarded alternative: hardcoding `"system"`, rejected because it would need to be swapped out later and could mask bugs where a real user should have been passed.
- **SKU uniqueness via DB unique index + pre-check** — the unique index is the final safety net against race conditions; the Create handler also pre-checks so it can return a clean business validation error instead of a raw DB constraint violation.
- **Soft delete via `IsDeleted` + EF Core Global Query Filter** — applies automatically to EF-based (write-path) queries; Dapper read queries must filter explicitly, since Dapper does not participate in EF's global filters (flagged as a risk below).
- **Offset-based pagination (`page`, `pageSize`)** — simple, sufficient for current expected data volume; can be revisited for cursor-based pagination if a future feature needs it.

## Risks

- **Dapper reads forgetting the `IsDeleted = false` filter** (not automatic, unlike EF's global filter) — mitigate with a shared base SQL fragment/constant and a test asserting soft-deleted products never appear in `GetProducts`/`GetProductById`.
- **Race condition on SKU uniqueness** between the pre-check and the insert — mitigated by the DB unique index; the Create handler must catch the resulting DB exception and translate it into the same business validation Result, not let it bubble up as an unhandled exception.
- **`CreatedBy`/`UpdatedBy` staying `null` indefinitely** if forgotten — tracked as a dependency to close when the Auth feature (backlog) is implemented.
