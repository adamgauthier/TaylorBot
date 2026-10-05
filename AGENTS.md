# Agent guidance

## Brevity and progressive disclosure

Keep instructions, documentation, and responses concise. Put essential guidance first and link to details near the relevant code. Read linked guidance when it applies to the task rather than loading every document upfront.

Keep this file a short entry point, not a duplicate of the architecture or tooling configuration. Avoid restating volatile values such as SDK versions and timeouts. Use one source line per Markdown paragraph and link directly to README files, not their containing folders.

Document the repository's steady-state usage and design, not session history, temporary workarounds, tiny specific detail or task status; link to authoritative code or configuration for details that could drift.

## Coding

Prefer focused changes and clean, readable, maintainable code. Follow nearby patterns and use modern C# features where they improve clarity.

Prefer collection expressions (`[]`), then target-typed `new()`, and use `var` otherwise. Name numeric/boolean literal arguments unless excluded by analyzer configuration, and use trailing commas where supported. Build analyzers and CI formatter checks catch configured style issues; follow [`.editorconfig`](./src/TaylorBot.Net/.editorconfig) and their diagnostics rather than duplicating the full rule set here.

Run the smallest relevant tests for code changes. Keep integration scenarios composed and readable, with real application behavior and stubbed external transports.

## Task-specific guidance

- [Repository overview](./README.md): architecture and local setup.
- [.NET development](./src/TaylorBot.Net/README.md): testing, code style, and background work.
- [Command integration tests](./src/TaylorBot.Net/Program.Commands.Discord/test/TaylorBot.Net.Commands.Discord.IntegrationTests/README.md): running and extending scenarios.
