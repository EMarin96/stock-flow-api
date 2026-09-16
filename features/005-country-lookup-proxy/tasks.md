# 005 · Country/State/City lookup proxy endpoints — Tasks

_Actionable checklist derived from `plan.md`. Small, concrete tasks; mark `[x]` once completed._

- [x] `src/Application/Countries/Shared/CountryErrors.cs` — `InvalidCountryCode`, `InvalidState`, `ReferenceDataUnavailable`.
- [x] `src/Application/Countries/Shared/CountryCodeParser.cs` — case-insensitive `Country` code parsing helper.
- [x] `src/Application/Countries/Shared/CountryDto.cs`, `StateDto.cs`, `CityDto.cs`.
- [x] `src/Application/Countries/GetCountries/` — `GetCountriesQuery` + Handler.
- [x] `src/Application/Countries/GetStates/` — `GetStatesQuery` + Handler.
- [x] `src/Application/Countries/GetCities/` — `GetCitiesQuery` + Handler.
- [x] `src/Api/Endpoints/CountryEndpoints.cs` — `MapCountryEndpoints` with the 3 GET routes, Swagger annotations, no special role restriction.
- [x] Wire `app.MapCountryEndpoints();` into `Program.cs`.
- [x] Unit tests: `GetCountriesHandlerTests`, `GetStatesHandlerTests`, `GetCitiesHandlerTests` (valid path, invalid country code, invalid state, reference-data-unavailable).
- [x] Integration tests: `tests/StockFlow.Tests/Api/Countries/CountryEndpointsTests.cs` (200s, 400s for bad codes, 401 with no token, 200 with a ReadOnly-role token).
- [x] Run `dotnet format` and `dotnet build`. Verified via PR #8's `build-and-test` CI check (restore, build, `dotnet format --verify-no-changes`) — all green.
- [x] Run `dotnet test` — full suite green. Verified via the same CI check (`dotnet test --no-build`).
- [x] Validate against the acceptance criteria in `spec.md`.
- [x] Move the feature to "Done" in `../../constitution/roadmap.md` and remove the corresponding bullet from "Backlog / ideas". Merged via [PR #8](https://github.com/EMarin96/stock-flow-api/pull/8).
