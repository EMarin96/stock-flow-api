# 003 · Stock movements

**Status:** proposed

## What it does

Users can register stock movements (`IN`, `OUT`, `TRANSFER`, `ADJUSTMENT`) for a product, list past movements with filtering and pagination, and browse a location's products with their current stock. Movements are the only way stock changes — there is no direct "set stock" operation, and once created a movement is never edited or deleted. Current stock is maintained as an up-to-date balance per product+location, updated atomically as a side effect of processing each movement — a client never sets it directly.

## Why

`StockMovement` is the core inventory-tracking mechanism described in `constitution/mission.md`: it's the only way stock changes, and every change is kept as a permanent, append-only audit trail. It depends on `Product` (001) and `Location` (002), both already implemented, and unblocks the backlog "Low-stock alerts" feature, which needs to know current stock to trigger a warning.

## Acceptance criteria

- [x] A movement can be created with a product, type (`IN`/`OUT`/`TRANSFER`/`ADJUSTMENT`), and a positive quantity.
- [x] `IN` requires a `DestinationLocationId`; `OUT` requires a `SourceLocationId`; `TRANSFER` requires both; `ADJUSTMENT` requires a `DestinationLocationId` (the location being corrected) and a `Direction` (`Increase`/`Decrease`).
- [x] Creating a movement with a location field that its type doesn't use (e.g. a `SourceLocationId` on an `IN`) is rejected with a clear validation error.
- [x] Creating a movement missing a location field its type requires is rejected with a clear validation error.
- [x] `Direction` is required when `Type` is `ADJUSTMENT` and rejected as invalid when present for any other type.
- [x] Creating a movement with a quantity that is zero or negative is rejected with a clear validation error.
- [x] Creating a movement referencing a product or location that doesn't exist (or is soft-deleted) is rejected with a clear validation error, not a generic failure.
- [x] A `TRANSFER` whose source and destination locations are in different countries is rejected with a clear validation error.
- [x] A movement that would leave the product's stock at a location negative (`OUT`, `TRANSFER` from that location, or `ADJUSTMENT`/`Decrease`) is rejected with a clear business error — stock never goes negative.
- [x] Movements can be listed with pagination, optionally filtered by product, location (matching either source or destination), and/or type.
- [x] A location's products and their current stock can be listed, paginated and optionally filtered by product name — including products whose stock is currently zero.
- [x] Creating a movement updates the affected location(s)' stock balance atomically, in the same operation as recording the movement — the two are never left inconsistent with each other.
- [x] Every movement record carries who created it and when — there is no "last updated," since movements are never edited.

## Out of scope

- Editing or deleting an existing movement — the log is append-only; corrections happen via a new inverse movement (hard limit in `constitution/tech-stack.md`).
- Low-stock alerts / notifications when stock crosses the minimum threshold — backlog feature, only consumes what this feature exposes.
- A "total stock across all locations" aggregate endpoint — current stock is always per product+location, matching the domain model (`tech-stack.md`: "stock is calculated per product + location combination, not globally").
- A single "get stock for one product at one location" lookup endpoint — superseded by the location-scoped listing above.
- Authentication and role-based access restrictions — deferred to the Auth feature (backlog), same as 001/002.
- A `Note`/`Reason` free-text field on movements.
