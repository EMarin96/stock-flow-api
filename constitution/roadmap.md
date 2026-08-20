# Roadmap

## Done ✅

1. **001 · Product management** — basic CRUD for products (SKU, minimum stock threshold).
2. **002 · Locations/warehouses** — location management and multi-warehouse support, with structured address (Country/State/City mandatory and validated against an external reference API; address lines independently optional).

## Next 🔜

_None yet._

## Backlog / ideas 💡

- **Authentication and user roles** — login + JWT + admin/operator/read-only roles.
- **Stock movements** — IN/OUT/TRANSFER/ADJUSTMENT, following the rules already defined in `tech-stack.md`.
- **Low-stock alerts** — notify when a product falls below its minimum stock threshold.
- **Reporting / read-only views** — reporting endpoints for the read-only role.
- **Country/State/City lookup proxy endpoints** — expose `GET /api/countries`, `/api/countries/{code}/states`, `/api/countries/{code}/states/{state}/cities` backed by the `ICountryReferenceDataService` cache introduced in 002, so the frontend can populate address dropdowns through this API instead of calling the external provider directly.

> Every new feature is created as `features/NNN-feature-name/` with `spec.md`, `plan.md`, and `tasks.md` before touching any code.
