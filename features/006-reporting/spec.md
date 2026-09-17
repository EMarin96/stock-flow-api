# 006 · Reporting / read-only views

**Status:** implemented ✅

## What it does

Two read-only reporting endpoints, available to any authenticated role (Admin, Operator, ReadOnly), that summarize data already captured by Products/Locations/StockMovements instead of requiring a caller to page through raw records and aggregate client-side:

- **Stock overview** (`GET /api/reports/stock-overview`) — one row per active product with its total quantity summed across every location, alongside its minimum stock threshold and whether it's currently at/below that threshold. Optionally filterable to only the products that are.
- **Movement activity** (`GET /api/reports/movement-activity`) — stock movements grouped by product and movement type (IN/OUT/TRANSFER/ADJUSTMENT) within an optional date range, each group showing how many movements occurred and their total quantity. Optionally filterable by product and/or location.

## Why

`constitution/mission.md`/`roadmap.md` calls for reporting endpoints for the read-only role — today a ReadOnly (or any) caller can only page through individual `Product`/`StockMovement`/`StockLevel` records one location or one page at a time, with no first-party way to see total stock per product across the whole warehouse network, or to summarize movement activity over time. Both reports reuse existing data (`StockLevel`, `StockMovement`, `Product`) with no new entity or write path.

## Acceptance criteria

- [x] `GET /api/reports/stock-overview` returns, for every active (non-deleted) product, its SKU, name, minimum stock threshold, and total quantity summed across all locations (a product with no stock anywhere shows `0`, not an omitted row).
- [x] Each stock-overview row flags whether the product is currently at or below its minimum stock threshold.
- [x] `GET /api/reports/stock-overview?lowStockOnly=true` returns only the products flagged as at/below threshold; omitting the parameter (or `false`) returns every active product.
- [x] `GET /api/reports/stock-overview` is paginated the same way as every other list endpoint (`page`/`pageSize`, capped, validated).
- [x] `GET /api/reports/movement-activity` returns one row per distinct product+movement-type combination that had at least one movement in the (optionally filtered) window, with the movement count and total quantity for that combination.
- [x] `GET /api/reports/movement-activity` can be filtered by `from`/`to` (a movement's `CreatedAt` falls within the range, either bound optional), `productId`, and/or `locationId` (matching either the source or destination, same convention as the existing movements list).
- [x] `GET /api/reports/movement-activity?from=...&to=...` with `from` after `to` is rejected with a clear validation error (400), not a generic failure or empty result.
- [x] `GET /api/reports/movement-activity` is paginated the same way as every other list endpoint.
- [x] Both endpoints are reachable by every role (Admin, Operator, ReadOnly) — no write access is required to read a report, consistent with every other GET endpoint in the API.
- [x] Both endpoints require authentication (401 with no/invalid token), same as every other endpoint.

## Out of scope

- Low-stock **alerts** (push notifications/emails when a product crosses its threshold) — a separate backlog feature; this feature only exposes a queryable snapshot, it does not notify anyone of anything.
- Exporting reports to CSV/PDF or any file format — JSON only, same as every other endpoint.
- Reports grouped by location for movement activity — a `TRANSFER` movement touches two locations at once, and grouping by a single location column would either double-count transfers or require an arbitrary tie-break; only product+type grouping is included in this feature. `locationId` remains available as a *filter* (not a `GROUP BY` key).
- Historical/point-in-time stock snapshots (e.g. "stock as of a past date") — `stock-overview` always reflects the current `StockLevel` balance.
- Any new role or permission beyond the existing three — reporting endpoints use the same fallback "any authenticated role" policy every other GET endpoint already uses, nothing new is introduced.
