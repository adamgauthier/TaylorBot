# TaylorBot

This repository is the home of the source code to TaylorBot, a multi-purpose Discord bot originally created for the r/TaylorSwift Discord server in **November 2015**. However, this source goes back to **September 2017**, when it was decided to do a complete rewrite. For an early code archive, see [TaylorBot.Classic](https://github.com/adamgauthier/TaylorBot.Classic).

## Components

TaylorBot is made up of multiple components, which are mostly built and run using [Docker](https://www.docker.com/) containers, connected through the use of a Docker network. This architecture allows most features to remain online while a component is experiencing downtime. The use of containers means you can run all components locally regardless of your OS environment, assuming you have Docker installed on your machine.

### postgres

[taylorbot-postgres](./src/taylorbot-postgres) is the [PostgreSQL](https://www.postgresql.org/)-based core database storing all persistent data. Note that while you can deploy this database locally using Docker, the production instance of TaylorBot uses a dedicated managed cloud service. The database schema is managed using a tool called [Sqitch](https://sqitch.org/), which you will need to run to create it as well as every time you make a change to it.

### redis-commands

[taylorbot-redis-commands](./src/linux-infrastructure/redis/redis-commands) is a [Redis](https://redis.io/) server used as a cache for heavily fetched data from `taylorbot-postgres`. Caching avoids frequent round-trips to the database, which could significantly impact performance.

### entity-tracker

[taylorbot-entity-tracker](./src/TaylorBot.Net) is a [.NET](https://dotnet.microsoft.com/) application based on [Discord.Net](https://github.com/discord-net/Discord.Net). Its main responsibilities are remembering usernames/joined dates, counting messages/minutes and logging member joins.

### user-notifier

[taylorbot-user-notifier](./src/TaylorBot.Net) is a [.NET](https://dotnet.microsoft.com/) application based on [Discord.Net](https://github.com/discord-net/Discord.Net). Its main responsibilities are sending out reminders, logging member leaves/bans, logging messages and notifying of new social posts.

### commands-discord

[taylorbot-commands-discord](./src/TaylorBot.Net) is a [.NET](https://dotnet.microsoft.com/) application based on [Discord.Net](https://github.com/discord-net/Discord.Net). Its main responsibilities are responding to interactions (slash commands) and legacy prefixed commands.

### linux-infrastructure

[linux-infrastructure](./src/linux-infrastructure) is not really a component itself, but simply a collection of linux scripts used to run and diagnose components. They can be used to deploy components locally, assuming you have some linux-based environment (like [WSL](https://docs.microsoft.com/en-us/windows/wsl/) on Windows).

More importantly, they are used on the production instance of TaylorBot, running on a Ubuntu cloud virtual machine. Components define `.yml` files that represent build and deploy steps for [Azure Pipelines](https://docs.microsoft.com/en-us/azure/devops/pipelines/), which is a cloud-based CI/CD service. These build steps are triggered automatically to validate code changes, while the deploy steps are triggered manually when a deployment is needed.

It's important to note that configurable secrets are stored using `.env` and `.pass`, which are used by these scripts. These files are obviously not present in the repository and need to be created manually with the desired values. Similar `.env.template` and `.pass.template` files that can be renamed are provided for convenience.

### slash-commands

[slash-commands](./src/slash-commands) is not really a component itself, but simply a collection of `.json` files representing all TaylorBot slash commands. To update available slash commands, a file is added or edited, then a deployment is made to push the `.json` contents to Discord's API.
