# Mission

## What we're building

An inventory management API, designed to scale in the future toward purchases and sales to customers. It's a simple, affordable alternative to heavy ERPs for small businesses.

1. **Products** — product catalog management.
2. **Stock movements** — tracking of stock in/out entries.
3. **Locations/warehouses** — multi-location/warehouse support.

## For whom

- Small businesses (owners/operators) looking for a simple, affordable alternative to heavy ERPs.
- Internal user roles: admin, warehouse operator, and read-only/reporting user — with room to add end customers once purchases/sales are added.
- Other stakeholders: the author, as the sole owner of the project.

## Principles

- **Quality and reliability** — strong test coverage, plus strict error handling and validation; nothing ships without both.
- **Clean, scalable architecture** — code stays maintainable long-term and is designed to grow toward a commercial, multi-tenant model.
- **Security by design** — role-based access control (admin, operator, read-only) is built in from the start, not bolted on later.
- **API-first, well-documented** — the API is treated as a product in itself, with clear, stable contracts.

## What it's NOT

- Not a full ERP — no accounting, payroll, or tax invoicing; inventory first, purchases/sales later.
- Not a visual interface/frontend — the project is the API itself, with no bundled dashboard or client app.
- Not built for large/enterprise businesses — not designed for needs like thousands of branches or complex legacy integrations.
- Not aiming to be generic for every industry — focused on small businesses with standard inventory needs, not industry-specific edge cases.
