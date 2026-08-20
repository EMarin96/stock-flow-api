# 002 · Locations/warehouses

**Status:** proposed

## What it does

Users can create, view (by ID and via a paginated list), edit, and delete (soft delete) locations/warehouses. Each location has a code, name, and address.

## Why

`Location` is a prerequisite for future features — `StockMovement` needs locations to exist for its `TRANSFER` operation (moving stock between two locations), and stock-per-location calculation depends on locations being defined first. Per `constitution/mission.md`, multi-location/warehouse support is one of the three core building blocks of the product.

## Acceptance criteria

- [ ] A location can be created with a code, name, and address.
- [ ] Creating a location with a code that already exists returns a clear validation error, not a generic failure.
- [ ] An existing location's fields can be edited (the code is not editable once created, matching the SKU-immutability convention from Product).
- [ ] A single location can be fetched by its ID.
- [ ] Locations can be listed with pagination.
- [ ] A location can be soft-deleted — it is never physically removed.
- [ ] Editing, deleting, or fetching a location ID that doesn't exist or is already soft-deleted returns a clear 404, not a server error.
- [ ] Every location record carries audit metadata (who/when created and last updated).

## Out of scope

- Stock quantity per location — calculated from `StockMovement` (backlog feature), not stored here.
- Location hierarchies (e.g. zones/aisles within a warehouse).
- A "default location" flag or concept.
- Authentication and role-based access restrictions — deferred to the Auth feature (backlog), same as Product.
