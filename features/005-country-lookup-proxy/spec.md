# 005 · Country/State/City lookup proxy endpoints

**Status:** in progress

## What it does

Authenticated callers can browse the supported countries and their real, recognized states and cities through StockFlow's own API — `GET /api/countries`, `GET /api/countries/{code}/states`, and `GET /api/countries/{code}/states/{state}/cities` — instead of calling the external country/state/city provider directly. This lets a frontend populate the country/state/city dropdowns used when creating or editing a `Location`'s address (feature 002) with values guaranteed to pass that same validation.

## Why

Feature 002 introduced mandatory, externally-validated `Country`/`State`/`City` fields on a `Location`'s address, but left the frontend with no first-party way to discover which values are valid — it would otherwise have to call the external provider directly, duplicating StockFlow's API key and caching logic outside its own backend. This was explicitly deferred to a backlog feature at the time (see `features/002-locations-warehouses/spec.md` — Out of scope). Exposing the data through StockFlow's own endpoints, backed by the same cache introduced in 002, closes that gap with no new external dependency.

## Acceptance criteria

- [ ] `GET /api/countries` returns the list of countries StockFlow currently supports (the closed `Country` set), with no path parameters.
- [ ] `GET /api/countries/{code}/states` returns the recognized states/provinces for a supported country code (e.g. `US`, `CR`), each with its ISO2 code and display name.
- [ ] `GET /api/countries/{code}/states/{state}/cities` returns the recognized cities for a given, valid country+state combination.
- [ ] An unsupported/unrecognized country `code` (on either endpoint that takes one) is rejected with a clear validation error (400), not a generic failure or a 404.
- [ ] A `state` that isn't recognized for the given country is rejected with a clear validation error (400) on the cities endpoint.
- [ ] Country/state code matching is case-insensitive (`us` and `US` behave the same), consistent with how `Location` address validation already matches state/city.
- [ ] If the underlying reference-data service is unavailable and nothing is cached yet for what was requested, the request is rejected with a clear `503`-style error rather than an empty list or a generic failure.
- [ ] All three endpoints require an authenticated caller (any role — Admin, Operator, or ReadOnly), same as every other read endpoint; no special write permission is needed since these are read-only.
- [ ] Results are served from the same cache `Location` address validation already relies on (see 002) — a values already warmed at startup or by prior use responds without a new external call.

## Out of scope

- Any new external API integration — this feature is a thin proxy over the `ICountryReferenceDataService`/cache already built in feature 002, not a new data source.
- Adding more countries than the two (`US`, `CR`) `Location` already supports — that's a change to the `Country` enum itself (feature 002), independent of this proxy.
- Exposing state/city by a stable numeric id instead of by name/ISO2 code — permanently out of scope for the same reason it's out of scope for `Location` (see `features/002-locations-warehouses/plan.md` — Decisions).
- Write access, filtering, search, or pagination on these lookup endpoints — the supported data set is small (a couple of countries, tens of states, at most a few hundred cities per state) and doesn't need it.
