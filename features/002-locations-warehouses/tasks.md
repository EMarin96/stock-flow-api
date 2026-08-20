# 002 · Locations/warehouses — Tasks

- [ ] Domain: create `LocationCode` value object (guard clauses via `DomainValidationException`) — `src/Domain/Locations/LocationCode.cs`
- [ ] Domain: create `Address` value object (`Street`/`City`/`Country`, all-or-nothing guard clause) — `src/Domain/Locations/Address.cs`
- [ ] Domain: create `Location : AuditableEntity` entity (`Code`, `Name`, `Address?`) — `src/Domain/Locations/Location.cs`
- [ ] Infrastructure: configure EF Core mapping for `Location` (unique index on `Code`, `Address` as an owned type, Global Query Filter `!IsDeleted`)
- [ ] Infrastructure: create and apply a new, additive EF Core migration (`AddLocationsTable`) for the Locations table
- [ ] Infrastructure: implement Dapper read queries for `GetLocationById` and `GetLocations` (paginated), filtering `IsDeleted = false`
- [ ] Application: `Shared/` — `ILocationWriteRepository`, `ILocationReadRepository`, `LocationDto`, `LocationMappingExtensions`, `LocationErrors`
- [ ] Application: implement `CreateLocation` slice (Command + Handler + Validator, including the "all or none" address rule)
- [ ] Application: implement `UpdateLocation` slice (Command + Handler + Validator)
- [ ] Application: implement `DeleteLocation` slice (Command + Handler, soft delete)
- [ ] Application: implement `GetLocationById` slice (Query + Handler)
- [ ] Application: implement `GetLocations` slice (Query + Handler + Validator for page/pageSize)
- [ ] Api: implement Minimal API endpoints for all 5 operations, wired to the Mediator — `src/Api/Endpoints/LocationEndpoints.cs`
- [ ] Api: add Swagger annotations/response types to each endpoint
- [ ] Tests: unit tests for each handler/validator, plus `LocationCodeTests`/`AddressTests` for the new value objects
- [ ] Tests: integration tests for the 5 endpoints, including the Code-uniqueness race-condition/DB-constraint-translation case, the soft-delete Dapper-filter case, and the partial-address validation case — reusing the shared `PostgresContainerFixture`
- [ ] Validate against the acceptance criteria in `spec.md`
- [ ] Move the feature to "Done" in `constitution/roadmap.md`
