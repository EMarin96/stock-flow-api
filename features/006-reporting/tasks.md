# 006 · Reporting / read-only views — Tasks

_Actionable checklist derived from `plan.md`. Small, concrete tasks; mark `[x]` once completed._

- [x] `src/Application/Reporting/Shared/StockOverviewDto.cs`, `MovementActivityDto.cs`.
- [x] `src/Application/Reporting/Shared/IReportingReadRepository.cs`.
- [x] `src/Application/Reporting/Shared/ReportingErrors.cs`.
- [x] `src/Application/Reporting/GetStockOverview/` — Query + Handler + Validator.
- [x] `src/Application/Reporting/GetMovementActivity/` — Query + Handler + Validator (including `From <= To` rule).
- [x] `src/Infrastructure/Persistence/Repositories/Reporting/ReportingReadRepository.cs` — Dapper implementation for both reports.
- [x] Register `IReportingReadRepository` in `src/Infrastructure/DependencyInjection.cs`.
- [x] `src/Api/Endpoints/ReportingEndpoints.cs` — both GET routes, Swagger annotations, no special role restriction.
- [x] Wire `app.MapReportingEndpoints();` into `Program.cs`.
- [x] `tests/StockFlow.Tests/TestDoubles/InMemoryReportingReadRepository.cs`.
- [x] Unit tests: `GetStockOverviewHandlerTests`, `GetMovementActivityHandlerTests`.
- [x] Integration tests: `tests/StockFlow.Tests/Api/Reporting/ReportingEndpointsTests.cs`.
- [ ] Run `dotnet format` and `dotnet build`. **Not run locally** — no .NET SDK/Docker daemon in this sandbox; relying on the PR's `build-and-test` CI check.
- [ ] Run `dotnet test` — full suite green. **Not run locally**, same reason; relying on CI.
- [ ] Validate against the acceptance criteria in `spec.md`.
- [ ] Move the feature to "Done" in `../../constitution/roadmap.md` and remove the corresponding bullet from "Backlog / ideas".
