# 002 · Locations/warehouses

**Status:** proposed

## What it does

Users can create, view (by ID and via a paginated list), edit, and delete (soft delete) locations/warehouses. Each location has a code, name, up to three free-text address lines (optional), and a country/state/city (required, must be a real, recognized combination).

## Why

`Location` is a prerequisite for future features — `StockMovement` needs locations to exist for its `TRANSFER` operation (moving stock between two locations), and stock-per-location calculation depends on locations being defined first. Per `constitution/mission.md`, multi-location/warehouse support is one of the three core building blocks of the product.

## Acceptance criteria

- [x] A location can be created with a code, name, country, state, and city (address lines are optional).
- [x] Creating a location with a code that already exists returns a clear validation error, not a generic failure.
- [x] An existing location's fields can be edited (the code is not editable once created, matching the SKU-immutability convention from Product).
- [x] A single location can be fetched by its ID.
- [x] Locations can be listed with pagination.
- [x] A location can be soft-deleted — it is never physically removed.
- [x] Editing, deleting, or fetching a location ID that doesn't exist or is already soft-deleted returns a clear 404, not a server error.
- [x] Every location record carries audit metadata (who/when created and last updated).
- [x] A location's address lines (up to three, free text) are optional and independent of one another.
- [x] Creating or editing a location without a country, state, and city is rejected with a clear validation error, not a generic failure.
- [x] An address with a state or city that isn't a real, recognized combination for the given country is rejected with a clear validation error, not a generic failure.
- [x] If the address reference-data service is unavailable when validation is needed, the write is rejected with a clear error rather than silently accepting unvalidated state/city values.

## Out of scope

- Stock quantity per location — calculated from `StockMovement` (backlog feature), not stored here.
- Location hierarchies (e.g. zones/aisles within a warehouse).
- A "default location" flag or concept.
- Authentication and role-based access restrictions — deferred to the Auth feature (backlog), same as Product.
- Exposing the country/state/city reference data as StockFlow's own endpoints for frontend dropdowns — deferred to a future backlog feature ("Country/State/City lookup proxy endpoints").
- Validating/storing cities by a stable ID instead of by name — permanently out of scope, not just deferred (see `plan.md` Decisions).
