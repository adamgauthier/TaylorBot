# .NET development

This folder contains the commands-discord and user-notifier applications and their shared libraries.

## Tests

Commands-discord and user-notifier have integration tests requiring Docker running Linux containers and the SDK specified in their application Dockerfiles. See the [commands-discord](./Program.Commands.Discord/test/TaylorBot.Net.Commands.Discord.IntegrationTests/README.md) and [user-notifier](./Program.UserNotifier/test/TaylorBot.Net.UserNotifier.IntegrationTests/README.md) guides. Shared Core/Commands unit tests remain in their `test` folders.

Use [Discord's OpenAPI specification](https://github.com/discord/discord-api-spec) to cross-check REST fixtures; follow the [developer docs](https://docs.discord.com/developers/intro) for Gateway events and when the preview specification disagrees.

Run an application's integration and shared tests from this folder:

```pwsh
dotnet test TaylorBot.Net.Commands.Discord.slnx
dotnet test TaylorBot.Net.UserNotifier.slnx
```

CI validates tests, formatting, and application builds. See [the shared pipeline](./build-test-publish-dotnet-docker.yml) for the current checks and configuration.

## Docker builds

The application build scripts use public NuGet packages without credentials. CI enables the private feed with `--build-arg USE_TAYLORBOT_FEED=true` and passes its existing feed token through `--secret id=FEED_ACCESS_TOKEN,env=FEED_ACCESS_TOKEN`. Docker builds require BuildKit; credentials are mounted only for restore-capable steps, not stored in image environment variables or credential-provider caches.

## Azure deployment

The [shared deployment script](./Deploy-TaylorBotNetComponent.ps1) accepts optional `TerminationGracePeriodSeconds` in `AzureConfigJson` or `AzureConfigFile`, as illustrated by the [commands-discord](./Program.Commands.Discord/template.commands-discord.Azure.json) and [user-notifier](./Program.UserNotifier/template.user-notifier.Azure.json) templates. Set it in each application's pipeline configuration to apply it on deployment; template changes alone do not update existing pipeline values. Omitting the property retains the previous deployment behavior.

Allow more time for [Azure's forced termination](https://learn.microsoft.com/en-us/azure/container-apps/application-lifecycle-management#shutdown) than the application's [cooperative shutdown budget](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.hostoptions.shutdowntimeout), leaving room for signal handling, cleanup, and process exit. Choose the margin from observed shutdown durations under load; increasing Azure's grace period does not extend the application's work-drain budget or guarantee recovery after a crash. Updating this revision-scoped setting causes a rollout.

## Code style

Prefer collection expressions (`[]`), then target-typed `new()`, and use `var` otherwise. Name numeric/boolean literal arguments unless excluded by analyzer configuration. Avoid blank lines between consecutive closing braces. Follow [`.editorconfig`](./.editorconfig), build analyzers, and the formatter checks defined in the shared pipeline.

## Background work

Register finite detached work with [`BackgroundTasks.Run`](./Core/src/TaylorBot.Net.Core/Tasks/BackgroundTasks.cs) to start inline or `Queue` to use the thread pool. Shutdown drains registered work, including nested tasks; failures are logged without abandoning siblings. A shutdown deadline cancels the wait, not the work. Do not register infinite producers.

User-notifier's [job coordinator](./Program.UserNotifier/src/TaylorBot.Net.UserNotifier.Program/Jobs/UserNotifierJobs.cs) owns scheduled loops separately, starting them once after Gateway readiness. Shutdown interrupts idle scheduling, waits for active cycles, then flushes queued tracking after Gateway and finite work have stopped. Cycles retain their own cadence and retry decisions; managed delays use `TimeProvider`.

Tracking flushes update PostgreSQL transactionally and restore server-rejected batches to Redis without overwriting newly queued work. Connection failures with an ambiguous commit outcome are reported rather than automatically replayed, which could duplicate increments.
