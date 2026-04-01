# Contributing to DHCPSwitches

Thanks for your interest in improving `DHCPSwitches`.

---

## Ground Rules

- Be respectful and constructive.
- Keep changes focused and atomic.
- Follow project roadmap priorities in `ROADMAP.md`.
- Use **Conventional Commits**.

---

## Development Workflow

1. Fork and create a feature branch:
   - `feat/<short-topic>`
   - `fix/<short-topic>`
   - `docs/<short-topic>`
2. Implement the change with minimal scope.
3. Build locally and validate behavior.
4. Open a Pull Request using the PR template.

---

## Commit Message Format

Use Conventional Commits:

- `feat: add next-free IP allocator`
- `fix: prevent duplicate IP assignment on auto-assign`
- `docs: update roadmap milestones`
- `chore: align project metadata`

Recommended structure:

```text
<type>(optional-scope): short summary
```

Types used in this repo:

- `feat`, `fix`, `docs`, `refactor`, `test`, `chore`

---

## Coding Guidelines

- Keep compatibility with current MelonLoader + IL2CPP interop patterns.
- Prefer clear, modular logic over large monolithic methods.
- Avoid introducing new dependencies unless strictly required.
- Preserve existing gameplay behavior unless the PR explicitly targets behavior changes.

---

## Documentation Guidelines

- Documentation files must be written in **English**.
- Use consistent Markdown structure with clear headings and separators.
- Update docs when behavior, setup, or UX changes.

---

## Pull Request Checklist

Before submitting, ensure:

- [ ] Build succeeds locally
- [ ] Changes are scoped and explained
- [ ] Docs are updated (if relevant)
- [ ] Commit messages follow Conventional Commits
- [ ] No unrelated refactors mixed in

---

## Reporting Bugs / Requesting Features

Use GitHub Issues and include:

- Game version
- Mod version / branch
- Repro steps
- Expected behavior
- Actual behavior
- Logs/screenshots if applicable
