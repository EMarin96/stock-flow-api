# 001 · Product management — Tasks

- [ ] Domain: create `Entity` base class (Id: Guid) — `src/Domain/Common/Entity.cs`
- [ ] Domain: create `AuditableEntity : Entity` (CreatedAt/CreatedBy, UpdatedAt/UpdatedBy, IsDeleted/DeletedAt) — `src/Domain/Common/AuditableEntity.cs`
- [ ] Domain: create `Product : AuditableEntity` entity (Sku, Name, Description, UnitOfMeasure, Price, MinimumStockThreshold) — `src/Domain/Products/Product.cs`
- [ ] Infrastructure: configure EF Core mapping for `Product` (unique index on Sku, Global Query Filter `!IsDeleted`)
- [ ] Infrastructure: create and apply EF Core migration for the Products table
- [ ] Infrastructure: implement Dapper read queries for `GetProductById` and `GetProducts` (paginated), filtering `IsDeleted = false`
- [ ] Application: implement `CreateProduct` slice (Command + Handler + Validator)
- [ ] Application: implement `UpdateProduct` slice (Command + Handler + Validator)
- [ ] Application: implement `DeleteProduct` slice (Command + Handler, soft delete)
- [ ] Application: implement `GetProductById` slice (Query + Handler)
- [ ] Application: implement `GetProducts` slice (Query + Handler + Validator for page/pageSize)
- [ ] Api: implement Minimal API endpoints for all 5 operations, wired to the Mediator — `src/Api/Endpoints/ProductEndpoints.cs`
- [ ] Api: add Swagger annotations/response types to each endpoint
- [ ] Tests: unit tests for each handler and validator
- [ ] Tests: integration tests for the 5 endpoints, including the SKU-uniqueness race-condition/DB-constraint-translation case and the soft-delete Dapper-filter case
- [ ] Validate against the acceptance criteria in `spec.md`
- [ ] Move the feature to "Done" in `constitution/roadmap.md`
