# 004 · Authentication and user roles

**Status:** proposed

## What it does

Users authenticate with a username and password to receive a JWT that identifies them and their role (`Admin`, `Operator`, `ReadOnly`) for a fixed period. Admins can create, list, view, update, and deactivate other users. Every existing endpoint (Products, Locations, StockMovements) now requires a valid token, and the role attached to that token determines what the caller may do. Every record created or updated from now on carries the identity of the authenticated user who did it.

## Why

`constitution/mission.md` states security by design: "role-based access control (admin, operator, read-only) is built in from the start, not bolted on later." Until now every endpoint has been open, and `AuditableEntity.CreatedBy`/`UpdatedBy` have stayed `null` on every record, by design, waiting for this feature. This is also the last piece needed before the API can be used by more than one trusted caller.

## Roles

A user has exactly one fixed role, per `constitution/tech-stack.md`:

- **Admin** — full access: everything Operator can do, plus deleting Products/Locations and managing users.
- **Operator** — day-to-day warehouse work: read everything, create/update Products and Locations, create StockMovements. Cannot delete Products/Locations, cannot manage users.
- **ReadOnly** — read-only access to Products, Locations, and StockMovements. No writes anywhere.

## Acceptance criteria

### Login
- [x] `POST /api/auth/login` with a valid username/password for an active user returns a JWT and its expiry, carrying the user's id, username, and role as claims.
- [x] An unknown username, a wrong password, or a deactivated user all return the same generic "invalid credentials" error (401) — the caller can't distinguish which case it was.
- [x] The token's expiry is configurable (`Jwt:ExpiryMinutes`), defaulting to 8 hours.

### Authorization on every endpoint
- [x] Every existing endpoint (Products, Locations, StockMovements) and every new Users endpoint rejects a request with no token or an invalid/expired token with `401 Unauthorized`.
- [x] A request with a valid token but a role not permitted for that endpoint is rejected with `403 Forbidden`, per the role table above (GET: all three roles; POST/PUT on Products/Locations/StockMovements: Admin+Operator; DELETE on Products/Locations: Admin only; all Users endpoints: Admin only).
- [x] StockMovements has no DELETE endpoint (unchanged, append-only) — nothing new to authorize there.

### User management (Admin only)
- [x] `POST /api/users` creates a user with a username, password, and role. Username must be unique (case-insensitive); a duplicate is rejected with a clear conflict error.
- [x] Passwords are never stored or returned in plain text — only a salted hash is persisted, and no response body (including the creator's) ever includes the password or its hash.
- [x] `GET /api/users` lists users, paginated, optionally filtered by username and/or role. Deactivated users are excluded by default (mirrors the existing soft-delete listing convention).
- [x] `GET /api/users/{id}` returns a single user by id, or `404` if it doesn't exist or is deactivated.
- [x] `PUT /api/users/{id}` can change a user's role and, optionally, reset their password (admin-set) in the same call. Username is not editable after creation.
- [x] `DELETE /api/users/{id}` deactivates the user (soft delete, same convention as Products/Locations — the row stays, `IsDeleted`/`DeletedAt` are set). A deactivated user can no longer log in.
- [x] An admin cannot deactivate their own account (`DELETE /api/users/{id}` on themselves is rejected with a clear error) — prevents locking out the only authenticated admin in the session.
- [x] Creating, updating, or deactivating a user that doesn't exist returns `404`.

### Bootstrap
- [x] On startup, if the `Users` table is empty, one `Admin` user is seeded from configuration (`Seed:AdminUsername`/`Seed:AdminPassword`). If the table is empty and that configuration is missing, the application fails to start with a clear error rather than starting with no way to log in.

### Audit trail
- [x] Every entity created or updated through an authenticated request (Products, Locations, StockMovements, Users) has its `CreatedBy`/`UpdatedBy` set to the acting user's id — no longer left `null`.

## Out of scope

- Refresh tokens — access-token only, per the confirmed decision; a caller logs in again once the token expires.
- Self-service "change my own password" — only an admin resetting another user's password via `PUT /api/users/{id}`.
- Multiple roles per user, or role hierarchies beyond the fixed three.
- Account lockout after repeated failed login attempts.
- Login/audit history (who logged in when) — only who created/updated a *record* is tracked, not login events.
- Email verification, OAuth/SSO, or any identity provider beyond username+password.
- Changing a user's username after creation.
