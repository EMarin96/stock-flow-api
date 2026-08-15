# 001 · Product management

**Status:** proposed

## What it does

Users can create, view (by ID and via a paginated list), edit, and delete (soft delete) products. Each product has a SKU, name, description, unit of measure, price, and a minimum stock threshold.

## Why

`Product` is the foundational entity of the whole inventory system — every other feature (stock movements, low-stock alerts, locations) depends on products already being defined. It replaces the manual/spreadsheet tracking described in `constitution/mission.md`.

## Acceptance criteria

- [ ] A product can be created with SKU, name, description, unit of measure, price, and minimum stock threshold.
- [ ] Creating a product with a SKU that already exists returns a clear validation error, not a generic failure.
- [ ] Creating or editing a product with a negative price is rejected with a validation error.
- [ ] Creating or editing a product with a negative minimum stock threshold is rejected with a validation error.
- [ ] An existing product's fields can be edited (the SKU is not editable once created).
- [ ] A single product can be fetched by its ID.
- [ ] Products can be listed with pagination.
- [ ] A product can be soft-deleted — it is never physically removed.
- [ ] Editing, deleting, or fetching a product ID that doesn't exist or is already soft-deleted returns a clear 404, not a server error.
- [ ] Every product record carries audit metadata (who/when created and last updated).

## Out of scope

- Current stock quantity/level — calculated from `StockMovement` (backlog feature).
- Authentication and role-based access restrictions — deferred to the Auth feature (backlog), per `constitution/roadmap.md`.
- Category as its own entity.
- Triggering low-stock alerts — this feature only stores the threshold; the backlog "Low-stock alerts" feature handles the trigger.
