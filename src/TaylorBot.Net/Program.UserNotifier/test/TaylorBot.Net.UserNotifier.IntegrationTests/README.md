# User-notifier integration tests

Scenarios run the real application with PostgreSQL, Redis, and the checked-in Sqitch schema. Synthetic Gateway events and strict Discord/HTTP transports replace external connections, not handlers, repositories, or Discord.Net clients.

## Running

With Docker running Linux containers and the .NET SDK pinned in [`global.json`](../../../global.json), run `dotnet test` here or `dotnet test TaylorBot.Net.UserNotifier.slnx` from the .NET root. No Discord credentials or manual database setup are needed.

For local Linux execution, run `.\Testing\Test-IntegrationLinux.ps1 -Application UserNotifier` from the .NET root. The [shared runner](../../../Testing/Test-IntegrationLinux.ps1) manages isolation and cleanup.

## Adding scenarios

Constructor-inject `DataServices` and create an `await using` [scenario](./Hosting/UserNotifierScenario.cs). Arrange with `Given`, `Feeds`, `Patreon`, and transport expectations; act through `Discord` or scheduled clock helpers; assert outgoing messages and `State`. Keep SQL, protocol payloads, and Discord.Net details in helpers so tests remain short, composed, and readable. See [reminders](./Notifications/ReminderTests.cs), [message tracking](./Tracking/MessageTrackingTests.cs), and [lifecycle](./Lifecycle/ScheduledJobsTests.cs).

Reuse the [shared support project](../../../Testing/TaylorBot.Net.IntegrationTests.Shared/TaylorBot.Net.IntegrationTests.Shared.csproj) rather than duplicating generic infrastructure, but keep application-specific behavior in each suite. Each suite owns independent containers; every scenario owns a fresh database and application host. Do not add test-to-test project references.

## Time and shutdown

Exercise scheduled work through scenario clock helpers, not direct domain-job calls. Fake time does not advance PostgreSQL time or Redis expiry, so arrange database eligibility relative to database time.

Gateway health scenarios enable `gatewayHealthLogs` when creating a scenario. Other scenarios retain errors but omit routine health summaries so large fake-time advances do not crowd their recent log history.

Clock advances normally acknowledge real Discord.Net heartbeats before running periodic health checks. Use `automaticHeartbeats: false` for explicit heartbeat failures and recovery, and `runUntilShutdown: true` to exercise the entry-point lifecycle and exit result without terminating the test runner. See the [watchdog scenarios](./Lifecycle/GatewayWatchdogTests.cs) for startup, partial-outage, resumption, and graceful-shutdown coverage.

Use transport gates rather than sleeps or replacement Discord.Net clients to simulate in-flight work; see the [lifecycle](./Lifecycle/ScheduledJobsTests.cs) and [tracking recovery](./Tracking/TrackingFailureTests.cs) scenarios.

Let scenario disposal own shutdown and cleanup. A shutdown timeout is a failure, not proof that work stopped; unsafe resources must not be reused. Unexpected requests, unused expectations, and application errors must fail scenarios. Declare intentional errors with narrowly matched, counted log expectations.
