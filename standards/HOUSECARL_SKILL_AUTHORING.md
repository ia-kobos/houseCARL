# houseCARL skill authoring standard

**Status:** binding for every skill shipped under `.agents/skills/`.

houseCARL skills carry domain knowledge and reusable workflows that do not belong in MCP tool
code. Keep the tool surface generic; use a skill for specialist grammar, decision procedure, or
reference material that improves how Codex chooses and composes tools.

## Layout

```text
.agents/skills/<skill-name>/
  SKILL.md
  references/     # optional
  scripts/        # optional
  assets/         # optional
  evals/          # optional, committed evaluation inputs only
```

- Folder names use `kebab-case`.
- `SKILL.md` starts with YAML frontmatter containing exactly the durable metadata Codex needs:
  `name` and `description`.
- `name` exactly matches the folder name.
- Supporting files use paths relative to the skill directory.
- Keep `SKILL.md` procedural and compact; move large lookup material into `references/`.

## When a skill should exist

Create a skill when the task needs at least one of:

- a reusable multi-step workflow;
- domain rules Codex must apply while reading tool output;
- a specialist grammar or reference corpus;
- safety or verification steps that span several MCP calls.

Do not create a skill for a one-step capability already described by one tool. Improve that
tool's description instead. Do not encode one modlist's facts in a shipped skill; those belong in
the modlist's own `AGENTS.md`, project skill, or project memory.

## Description rules

The description is routing metadata. It must say both what the skill does and when to use it.

- Lead with an action: `Author`, `Diagnose`, `Inspect`, `Review`, `Look up`, or similar.
- Use words a modder will actually say: record types, tools, filenames, symptoms, and output
  formats.
- Name close boundaries when a neighboring skill would otherwise be selected.
- Avoid internal phase names, implementation history, and conditions Codex cannot infer from the
  user's request.
- Keep it short enough to scan alongside every other installed skill.

## Body rules

- Start with the workflow, not background history.
- Use real tool and argument names. Never invent syntax from memory.
- Read before writing. State the narrowest safe write path and the verification step.
- Keep houseCARL's cornerstones intact: complete coverage by construction, generic composition
  primitives, loud failure, and non-destructive writes by default.
- Reference supporting files only when the current task needs them.
- Do not duplicate long reference data in `SKILL.md`.

## Validation

Every new or changed skill must satisfy:

1. The folder name and frontmatter `name` match.
2. YAML frontmatter parses and contains a non-empty `description`.
3. Every referenced local file exists.
4. The skill is included in `scripts/build-plugin.ps1` when it ships.
5. `plugin-validate-guard` passes.
6. `codex-umbrella-coverage-guard` passes when tools or helper skills change.
7. At least one representative fresh-context invocation routes to the skill and follows its first
   required action. Visibility alone is not activation proof.

Keep committed eval sets under `evals/`; keep run output and private modlist evidence untracked.

## Review checklist

- Is a skill actually needed?
- Is the description concrete and user-recognizable?
- Does the body preserve current tool names and safety contracts?
- Is domain knowledge in the skill instead of a bespoke MCP verb?
- Are large references separated from the workflow?
- Are private paths, load-order details, credentials, and session transcripts absent?
- Do the packaging and coverage guards pass?
