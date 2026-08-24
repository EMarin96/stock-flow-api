# 004 · Authentication and user roles — Plan

## Approach

New `Users` vertical (Domain/Application/Infrastructure/Api) plus a small `Auth` slice for login, following the same CQRS/vertical-slice shape as 001-003. Passwords are hashed with PBKDF2 (`Rfc2898DeriveBytes`, BCL — no new package); JWTs are created and validated with the standard ASP.NET Core JWT stack (two new packages — see Decisions). Three cross-cutting abstractions live in `Application/Common/Security/`: `IPasswordHasher`, `IJwtTokenGenerator`, `ICurrentUserService` — Domain and Application stay free of ASP.NET Core/cryptography-library specifics, same layering already used for `ICountryReferenceDataService`.

Authorization is **fail-closed by default**: a global `FallbackPolicy` requires an authenticated user on every endpoint; `POST /api/auth/login` is the one explicit `AllowAnonymous()` exception. Two named policies (`WriteAccess` = Admin+Operator, `AdminOnly` = Admin) are applied per-endpoint on top of that default for the routes that need more than "any authenticated user."

Every existing `Create`/`Update` entity method gains a `Guid? createdBy`/`updatedBy` parameter, sourced from `ICurrentUserService.UserId` in each handler — closing the gap `AuditableEntity`'s doc comment has flagged since 001.

## Implementation

### Domain
1. `src/Domain/Users/Role.cs` — enum `Admin`/`Operator`/`ReadOnly`.
2. `src/Domain/Users/User.cs : AuditableEntity` — `Username`, `NormalizedUsername` (private set, `username.ToUpperInvariant()`, computed inside `Create` — mirrors ASP.NET Identity's normalized-username pattern so uniqueness is case-insensitive without a Postgres-specific `citext` column), `PasswordHash`, `Role`. `Create(username, passwordHash, role, createdBy)` guard clauses via `DomainValidationException` (username non-empty, max length). `UpdateRole(role, updatedBy)`, `ResetPassword(passwordHash, updatedBy)` — two separate behavior methods since `PUT /api/users/{id}` can call either or both. Reuses inherited `SoftDelete()` for deactivation, same as `Product`/`Location`.
3. Retrofit `Guid? createdBy` into `Product.Create`, `Guid? updatedBy` into `Product.Update`; same pair for `Location.Create`/`Update`; `Guid? createdBy` into `StockMovement.Create` (no `Update` — append-only).

### Application
4. `src/Application/Common/Security/`: `IPasswordHasher` (`Hash(password)`, `Verify(password, hash)`), `IJwtTokenGenerator` (`GenerateToken(Guid userId, string username, Role role) → (string Token, DateTime ExpiresAt)`), `ICurrentUserService` (`Guid? UserId`, `Role? Role`).
5. `src/Application/Users/Shared/`: `IUserWriteRepository` (`UsernameExistsAsync`, `GetByIdAsync`, `AddAsync`), `IUserReadRepository` (`GetByIdAsync → UserDto?`, `GetPagedAsync(page, pageSize, usernameFilter?, role?) → PagedResult<UserDto>`, `GetForAuthenticationAsync(username) → AuthenticationRecord?` — a dedicated projection carrying `Id`/`Username`/`PasswordHash`/`Role`/`IsDeleted`, kept separate from `UserDto` so a password hash can never leak into a Users CRUD response), `UserDto` (Id, Username, Role, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy — **no password field**), `UserErrors` (`UsernameAlreadyExists`, `NotFound`, `CannotDeactivateSelf`, `ValidationFailed`), `UserMappingExtensions.ToDto()`.
6. `src/Application/Users/CreateUser/` — `CreateUserCommand(Username, Password, Role)` + validator (Username non-empty/max length, Password min length 8, Role `IsInEnum()`) + handler: validate → `UsernameExistsAsync` pre-check (`UsernameAlreadyExists`) → `passwordHasher.Hash(...)` → `User.Create(..., currentUserService.UserId)` → `AddAsync` + `SaveChangesAsync`, catching `DbUpdateUniqueConstraintException` as the same `UsernameAlreadyExists` (race-condition backstop, same dual-guard pattern as SKU in 001).
7. `src/Application/Users/GetUsers/` — paginated list, optional `usernameFilter`/`role` filters, excludes soft-deleted (same convention as `GetProducts`).
8. `src/Application/Users/GetUserById/` — 404 via `UserErrors.NotFound`.
9. `src/Application/Users/UpdateUser/` — `UpdateUserCommand(Id, Role, NewPassword?)` + handler: load user (404 if missing), `UpdateRole(...)`, and if `NewPassword` present, hash + `ResetPassword(...)`.
10. `src/Application/Users/DeactivateUser/` — `DeactivateUserCommand(Id)` + handler: 404 if missing; if `Id == currentUserService.UserId` → `UserErrors.CannotDeactivateSelf`; else `SoftDelete()`.
11. `src/Application/Auth/Login/` — `LoginCommand(Username, Password)` + handler: `userReadRepository.GetForAuthenticationAsync(username)`; if null, deactivated, or `passwordHasher.Verify(...)` fails → the same generic `AuthErrors.InvalidCredentials` in every case (no distinguishable signal for enumeration); else `jwtTokenGenerator.GenerateToken(...)` → `LoginResponseDto(Token, ExpiresAt)`.
12. `Application/Common/Results/Error.cs` — add `ErrorType.Unauthorized` + `Error.Unauthorized(code, message)` factory, for `AuthErrors.InvalidCredentials`.
13. Retrofit `ICurrentUserService currentUserService` into `CreateProductHandler`/`UpdateProductHandler`/`CreateLocationHandler`/`UpdateLocationHandler`/`CreateStockMovementHandler`, passing `.UserId` into the corresponding entity call.

### Infrastructure
14. `src/Infrastructure/Security/PasswordHasher.cs` — PBKDF2 (`Rfc2898DeriveBytes.Pbkdf2`, SHA-256, 100k iterations, random 16-byte salt), stored as `"{iterations}.{base64 salt}.{base64 hash}"`; `Verify` recomputes and compares with `CryptographicOperations.FixedTimeEquals` (constant-time, avoids a timing side-channel on password checks).
15. `src/Infrastructure/Security/JwtOptions.cs` — Options Pattern (`SectionName = "Jwt"`), `Secret`, `Issuer` (default `"StockFlowApi"`), `Audience` (default `"StockFlowApi"`), `ExpiryMinutes` (default `480`, i.e. 8h). `Secret` is a genuine secret — same treatment as `CountryStateCityOptions.ApiKey`: empty placeholder in `appsettings.Development.json`, real value via `dotnet user-secrets set "Jwt:Secret" "<value>"` locally, env var/CI secret elsewhere.
16. `src/Infrastructure/Security/JwtTokenGenerator.cs` — implements `IJwtTokenGenerator` using `System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler`; claims `sub`/`ClaimTypes.NameIdentifier` = user id, `ClaimTypes.Role` = `role.ToString()` (so `[Authorize(Roles=...)]`/`RequireRole` work with zero extra mapping), `ClaimTypes.Name` = username; signed with `HmacSha256` using `JwtOptions.Secret`.
17. `src/Infrastructure/Persistence/Configurations/UserConfiguration.cs` — table `Users`; `Username`/`NormalizedUsername` `HasMaxLength(100)`, unique index on `NormalizedUsername`; `PasswordHash` `HasMaxLength(256)`; `Role` `HasConversion<string>()`; `HasQueryFilter(u => !u.IsDeleted)` (same soft-delete convention as `Product`/`Location`).
18. `src/Infrastructure/Persistence/Repositories/Users/{UserWriteRepository.cs, UserReadRepository.cs}` — same Write(EF)/Read(Dapper) split and folder convention as every other feature; `UserReadRepository`'s Dapper queries filter `"IsDeleted" = false` explicitly (does not participate in EF's query filter, same documented caveat as `ProductReadRepository`) and never select `PasswordHash` except in `GetForAuthenticationAsync`.
19. Migration `AddUsersTable`.
20. `DependencyInjection.cs` — register `IUserWriteRepository`/`IUserReadRepository`, `IPasswordHasher`, `IJwtTokenGenerator`, `Configure<JwtOptions>(...)`.

### Api
21. `src/Api/Security/CurrentUserService.cs` — implements `ICurrentUserService` via `IHttpContextAccessor` (reads the authenticated `ClaimsPrincipal`'s claims). Registered in `Program.cs` (`AddHttpContextAccessor()` + `AddScoped<ICurrentUserService, CurrentUserService>()`) — Api-layer concern, same reasoning already documented for `AddHttpClient`/`AddMemoryCache`/`AddHostedService`.
22. `src/Api/Endpoints/AuthEndpoints.cs` — `POST /api/auth/login`, `.AllowAnonymous()`.
23. `src/Api/Endpoints/UserEndpoints.cs` — `POST/GET/{id}/GET-list/PUT/{id}/DELETE/{id}` under `/api/users`, every route `.RequireAuthorization("AdminOnly")`.
24. `Program.cs`:
    - `builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));`
    - `builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => { /* TokenValidationParameters from JwtOptions */ });`
    - `builder.Services.AddAuthorization(options => { options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(); options.AddPolicy("WriteAccess", p => p.RequireRole(nameof(Role.Admin), nameof(Role.Operator))); options.AddPolicy("AdminOnly", p => p.RequireRole(nameof(Role.Admin))); });`
    - `app.UseAuthentication(); app.UseAuthorization();` after `app.UseExceptionHandler()`, before endpoint mapping.
    - `app.MapAuthEndpoints(); app.MapUserEndpoints();` alongside the existing `Map...Endpoints()` calls.
    - **Startup admin bootstrap**: a scoped block (same shape as the existing Development-only migration block, but unconditional — real environments apply migrations separately, so the `Users` table already exists by the time this runs) — if `IUserWriteRepository` reports no users at all (a raw count, bypassing the soft-delete filter), read `Seed:AdminUsername`/`Seed:AdminPassword` from configuration; throw `InvalidOperationException` if either is missing (same fail-fast style as the connection-string check in `AddInfrastructure`); else hash the password and create the admin user (`createdBy: null` — no authenticated actor exists yet).
25. Retrofit the existing endpoint files:
    - `ProductEndpoints.cs`/`LocationEndpoints.cs`/`StockMovementEndpoints.cs`: `POST`/`PUT` routes get `.RequireAuthorization("WriteAccess")`; `DELETE` routes (Products/Locations only) get `.RequireAuthorization("AdminOnly")`; `GET` routes need no extra call — the `FallbackPolicy` already requires authentication, and no route restricts *which* role, satisfying "all three roles can read."
26. Swagger: add JWT bearer auth support (`AddSecurityDefinition`/`AddSecurityRequirement` in `AddSwaggerGen`) so the Swagger UI can send a token — this is normal boilerplate for the existing `AddSwaggerGen` block, not a new package.

### Configuration
27. `appsettings.Development.json` — add `"Jwt": { "Issuer": "StockFlowApi", "Audience": "StockFlowApi", "ExpiryMinutes": 480, "Secret": "" }` and `"Seed": { "AdminUsername": "", "AdminPassword": "" }` (empty placeholders; real dev values via `dotnet user-secrets`, matching `CountryStateCity:ApiKey`'s precedent — these two are genuine secrets too, unlike the local Postgres password).

### Tests
28. `tests/StockFlow.Tests/Api/ApiFactory.cs`/`ApiFactoryFixture.cs` gain a way to obtain a bearer token per role for integration tests (e.g. seed a test user of each role and call `/api/auth/login`, or mint a token directly via `IJwtTokenGenerator` from the test host's DI container) — every existing `ProductEndpointsTests`/`LocationEndpointsTests`/`StockMovementEndpointsTests`/`LocationStockEndpointTests` call needs an authorized client instead of the current anonymous one; new tests cover 401 (no/invalid token) and 403 (wrong role) explicitly per endpoint group, plus the Users/Auth endpoints themselves.
29. Domain tests: `User.Create`/`UpdateRole`/`ResetPassword` guard clauses. Application tests: `PasswordHasher` round-trip + wrong-password rejection, `JwtTokenGenerator` claim contents, every Users/Auth handler (including `CannotDeactivateSelf` and the generic invalid-credentials path).

## Decisions

- **PBKDF2 over BCrypt/Identity's hasher** — zero new NuGet packages for this part, sidesteps the tech-stack.md "no new dependency without checking" hard limit entirely for hashing; `Rfc2898DeriveBytes.Pbkdf2` + constant-time comparison is a well-established, still-recommended approach for password hashing in .NET.
- **Two new NuGet packages, not one** — correcting the plan-mode estimate: `Microsoft.AspNetCore.Authentication.JwtBearer` (Api — validates incoming tokens) and `System.IdentityModel.Tokens.Jwt` (Infrastructure — creates them; Infrastructure doesn't reference Api, so it needs its own token-writing package). Both are the standard, first-party Microsoft packages for this exact job — flagged here as the deliberate, approved exception to the hard limit.
- **`NormalizedUsername` for case-insensitive uniqueness** — mirrors ASP.NET Core Identity's own convention, avoids a Postgres-specific `citext` extension, and keeps the pre-check + DB-unique-index dual-guard pattern (already used for SKU/location code) working unchanged.
- **Fail-closed by default (`FallbackPolicy` + explicit `AllowAnonymous()` on Login only)** — a future endpoint added without an explicit authorization call is still protected (any authenticated user, any role) rather than silently open; the role table in spec.md/tasks.md pins down which endpoints need a *stricter* policy on top.
- **`ICurrentUserService`/`IJwtTokenGenerator`/`IPasswordHasher` are Application interfaces, implemented in Api/Infrastructure** — keeps Domain and Application free of ASP.NET Core (`HttpContext`) and cryptography-library specifics, same boundary already drawn for `ICountryReferenceDataService`.
- **`GetForAuthenticationAsync` returns a dedicated record, not `UserDto`** — guarantees a password hash can never accidentally end up serialized in a Users CRUD response; the only code path that ever reads `PasswordHash` is login.
- **Password reset is admin-only, via `PUT /api/users/{id}`** — no self-service "change my own password" in this feature, per spec's Out of scope; kept as two separate `User` behavior methods (`UpdateRole`/`ResetPassword`) so the handler only calls what the request actually asked for.
- **CreatedBy/UpdatedBy retrofit touches every existing Create/Update handler and entity** — real, acknowledged scope addition (not scope creep): `AuditableEntity`'s doc comment has named this feature as the one to close that gap since it was written in 001.

## Risks

- **Existing 236 tests break on first run** once retrofit lands, since every current integration test call is anonymous — expected and tracked as its own implementation step (see Implementation #28), not a regression to chase down later.
- **JWT secret/seed admin password must never be committed** — mitigated by following the exact `CountryStateCityOptions.ApiKey` precedent (empty placeholder, user-secrets/env var for a real value); `features-verifier` should double-check no real value slipped into `appsettings.Development.json` before merge.
- **No token revocation before natural expiry** — accepted consequence of the "access-token only, no refresh" decision; the only mitigation is keeping the default expiry short-ish (8h) rather than long-lived.
