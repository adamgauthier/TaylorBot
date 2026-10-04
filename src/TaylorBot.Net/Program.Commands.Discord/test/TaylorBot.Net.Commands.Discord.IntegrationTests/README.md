# Commands-discord integration tests

Tests send synthetic Discord Gateway events through the real application and inspect outgoing Discord requests. Handlers, repositories, PostgreSQL, and Redis remain real; external transports are stubbed.

## Running

With the SDK specified in the [application Dockerfile](../../Dockerfile) and Docker running Linux containers, run from this folder:

```pwsh
dotnet test
```

No Discord credentials or manual database setup are needed. The suite manages isolated containers, applies the checked-in Sqitch schema, and gives each scenario fresh application and data state. Existing development resources are untouched.

`dotnet test` runs on your current OS; CI runs it directly on a Linux agent. For a local Linux run, use the [Linux test script](../../Test-CommandsDiscordLinux.ps1), which runs the commands-discord solution using the Dockerfile's SDK image on an isolated Docker network and cleans up afterward.

## Design principles

- **Test through real boundaries.** Send raw Discord events through normal application startup, routing, and execution with real PostgreSQL and Redis. Stub only external transports, not repositories or command services; never invoke commands directly.
- **Compose, don't inherit.** Tests should be as simple as small unit tests: linear arrange/act/assert, evident from names and spacing without explanatory comments. Keep setup boilerplate, SQL, JSON, and SDK details in reusable helpers, not branches or loops in test methods.
- **Assert behavior, not implementation.** Check outgoing Discord responses and relevant persisted effects, not internal calls or cosmetic emoji/Markdown. Keep seed data and read-back queries independent of production repositories, and SDK-specific glue outside the scenario API.
- **Keep scenarios isolated and failures visible.** Use synthetic fixtures and fresh state, await command completion, and drain owned work before cleanup. Unexpected requests, unused external responses, errors, and timeouts must fail rather than silently pass.

## Fixtures

Discord and external API fixtures stay synthetic. Follow Discord's [interaction](https://docs.discord.com/developers/interactions/receiving-and-responding) and [message](https://docs.discord.com/developers/resources/message) contracts, preserving context, optional fields, and request/response differences; [payload checks](./Framework/DiscordPayloadTests.cs) guard these shapes. `ScenarioDm` preserves DM recipients and installation context across interactions.

To confirm real API shapes, temporarily instrument a local development bot and capture traffic. Protect captures outside the repository, redact credentials, and review sanitized payloads alongside warnings and failures. Use the findings to improve synthetic fixtures, then remove temporary instrumentation and captures after review; never commit captured traffic.

## Adding tests

Add a class under `Commands/<feature>` (or `Framework` for cross-command behavior), constructor-inject `DataServices`, and create an `await using` scenario as shown in the [balance tests](./Commands/Taypoints/TaypointsBalanceTests.cs). Arrange through `Given` and `External`, act through `Discord`, and assert the response plus any relevant `State` effects; extend shared helpers when needed rather than adding plumbing to the test (see [succession tests](./Commands/Taypoints/SuccessionTests.cs) for components and [image tests](./Commands/Image/ImageTests.cs) for external responses).

Declare each deployed route genuinely exercised with `[Trait("Command", "taypoints balance")]` on its test class or method; the coverage guard compares these declarations against global and guild command definitions. Cover meaningful success, important rejection paths, and returned component flows rather than just checking that a command responds; disabled commands and obsolete migration-only aliases are outside this scope.
