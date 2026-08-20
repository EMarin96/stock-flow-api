# Git conventions

## Branching strategy

- `main` — production, always deployable.
- `development` — integration branch; every feature or fix branches off it.

## Branch naming

- `feature/NNN-feature-name`, matching the corresponding `features/NNN-feature-name/` folder (e.g. `feature/001-product-management`), for code.
- `docs/NNN-feature-name`, for a feature's `spec.md`/`plan.md`/`tasks.md` PR, before the code branch is cut.

## When to branch

- `main` and `development` are protected — every change to them, including documentation, goes through a PR (no direct pushes, not even for the repo owner/admin).
- Once `spec.md`, `plan.md`, and `tasks.md` are written and approved, open a short-lived `docs/NNN-feature-name` PR into `development` for them.
- After that docs PR is merged, cut `feature/NNN-feature-name` off `development` for the actual code.

## Commit messages

- Conventional Commits: `feat:`, `fix:`, `chore:`, `docs:`, `refactor:`, `test:`, etc.

## Pull requests

- Every merge into `development` or `main` goes through a PR.
- `feature/*` → `development`: squash merge — one clean commit per feature.
- `development` → `main`: regular merge commit, tagged with a version.

## Before merging

- CI (build + tests) must be green.
- The PR description must reference its corresponding `features/NNN-.../` folder.

## Versioning

- Semantic Versioning (`MAJOR.MINOR.PATCH`), tagged on `main` at each release.
- `feat` → minor bump, `fix` → patch bump, breaking change → major bump.

## Protected branches

- `main` and `development` — no direct pushes, PR only.
- `feature/*` branches — direct pushes allowed.
