# .NET development

This folder contains the commands-discord and user-notifier applications and their shared libraries.

## Tests

Commands-discord has [integration tests](./Program.Commands.Discord/test/TaylorBot.Net.Commands.Discord.IntegrationTests/README.md) requiring Docker running Linux containers and the SDK specified in the [application Dockerfile](./Program.Commands.Discord/Dockerfile). Shared Core/Commands and user-notifier unit tests live in their respective `test` folders.

Run commands-discord's integration and shared tests from this folder:

```pwsh
dotnet test TaylorBot.Net.Commands.Discord.slnx
```

CI validates tests, formatting, and application builds. See [the shared pipeline](./build-test-publish-dotnet-docker.yml) for the current checks and configuration.

## Docker builds

The application build scripts use public NuGet packages without credentials. CI enables the private feed with `--build-arg USE_TAYLORBOT_FEED=true` and passes its existing feed token through `--secret id=FEED_ACCESS_TOKEN,env=FEED_ACCESS_TOKEN`. Docker builds require BuildKit; credentials are mounted only for restore-capable steps, not stored in image environment variables or credential-provider caches.

## Code style

Prefer collection expressions (`[]`), then target-typed `new()`, and use `var` otherwise. Name numeric/boolean literal arguments unless excluded by analyzer configuration. Avoid blank lines between consecutive closing braces. Follow [`.editorconfig`](./.editorconfig), build analyzers, and the formatter checks defined in the shared pipeline.

## Background work

Register finite detached work with [`BackgroundTasks.Run`](./Core/src/TaylorBot.Net.Core/Tasks/BackgroundTasks.cs) to start inline or `Queue` to use the thread pool. Shutdown drains registered work, including nested tasks; failures are logged without abandoning siblings. A shutdown deadline cancels the wait, not the work. Do not register infinite producers.
