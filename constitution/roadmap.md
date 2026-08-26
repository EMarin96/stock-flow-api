# Roadmap

## Done ✅

1. **001 · Product management** — basic CRUD for products (SKU, minimum stock threshold).
2. **002 · Locations/warehouses** — location management and multi-warehouse support, with structured address (Country/State/City mandatory and validated against an external reference API; address lines independently optional).
3. **003 · Stock movements** — IN/OUT/TRANSFER/ADJUSTMENT movements, materialized per-product+location `StockLevel` balance updated atomically with each movement, movement listing and location-scoped stock listing.
4. **004 · Authentication and user roles** — username/password login issuing a JWT (id/username/role claims, configurable expiry), fail-closed authorization on every endpoint (admin/operator/read-only role table), Admin-only Users CRUD (create/list/get/update/deactivate, salted-hash passwords, case-insensitive unique usernames), startup admin bootstrap from configuration with fail-fast if missing, and `CreatedBy`/`UpdatedBy` audit trail wired into every existing write across Products/Locations/StockMovements.

## Next 🔜

_None yet._

## Backlog / ideas 💡

- **Low-stock alerts** — notify when a product falls below its minimum stock threshold.
- **Reporting / read-only views** — reporting endpoints for the read-only role.
- **Country/State/City lookup proxy endpoints** — expose `GET /api/countries`, `/api/countries/{code}/states`, `/api/countries/{code}/states/{state}/cities` backed by the `ICountryReferenceDataService` cache introduced in 002, so the frontend can populate address dropdowns through this API instead of calling the external provider directly.

> Every new feature is created as `features/NNN-feature-name/` with `spec.md`, `plan.md`, and `tasks.md` before touching any code.
