# Roadmap

## Done ✅

1. **001 · Product management** — basic CRUD for products (SKU, minimum stock threshold).

## Next 🔜

1. **002 · Locations/warehouses** — location management and multi-warehouse support.

## Backlog / ideas 💡

- **Authentication and user roles** — login + JWT + admin/operator/read-only roles.
- **Stock movements** — IN/OUT/TRANSFER/ADJUSTMENT, following the rules already defined in `tech-stack.md`.
- **Low-stock alerts** — notify when a product falls below its minimum stock threshold.
- **Reporting / read-only views** — reporting endpoints for the read-only role.

> Every new feature is created as `features/NNN-feature-name/` with `spec.md`, `plan.md`, and `tasks.md` before touching any code.
