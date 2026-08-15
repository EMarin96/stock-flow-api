# spec/ — Spec Driven Development (template)

> Generic template for documenting any project using specification-driven development (SDD): the spec is written first, then the plan, then the tasks, and only then is the code touched.
>
> **How to use this template:** copy this folder into your project as `spec/`, fill in `constitution/` once at the start, and create one folder per feature from `features/NNN-feature-name/`. Replace everything between `<…>` and delete the notes in _italics_.

## Structure

```
spec/
├── constitution/            ← stable project rules (change rarely)
│   ├── mission.md           ← what we're building and for whom
│   ├── tech-stack.md        ← technologies, conventions, and limits
│   ├── git-conventions.md   ← branching, commits, PRs, versioning
│   └── roadmap.md           ← order of the features
└── features/                ← one folder per feature
    └── NNN-feature-name/
        ├── spec.md          ← what it does + acceptance criteria
        ├── plan.md          ← how it's implemented
        └── tasks.md         ← task checklist
```

_The constitution can be a single file if the project is small; each feature can also be a single file. Split it up as it grows._

## Workflow for a new feature

1. Create `features/NNN-feature-name/` with the next free number (`001`, `002`, …).
2. Write `spec.md`: what it does, why, and measurable acceptance criteria.
3. Write `plan.md`: technical approach and decisions, respecting `constitution/tech-stack.md`.
4. Break it down in `tasks.md` and track progress.
5. Implement and validate (build/tests/lint, or whatever the constitution defines).
6. Update `constitution/roadmap.md` (move the feature to "Done").

> The constitution rules: if a feature conflicts with `mission.md` or `tech-stack.md`, the feature is rethought, not the constitution.
