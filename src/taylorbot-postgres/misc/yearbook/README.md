# Yearbook

Scripts for generating yearly activity stats for a Discord server. Uses database dumps from two consecutive years of [taylorbot-postgres](../../) to calculate activity metrics, rank users, and generate personalized recap images. For example, in our main server, dumps are taken on server anniversary (November 22) every year to run these scripts.

## Setup

Make sure you have [.NET](https://dotnet.microsoft.com/) and [Docker](https://www.docker.com/) installed.

## Steps

### 1. Configure

Create a copy of [`template.yearbook.json`](template.yearbook.json) and rename it to `yearbook.json`. Set `year` to the yearbook year, `dumpMonth` and `dumpDay` to the date the dumps were taken (e.g. `11` and `22` for the November 22 server anniversary), and configure database connection details for both the previous and current year dumps.

### 2. Deploy Databases

Deploy local PostgreSQL instances and restore database backup dumps for both the previous and current year. Ports and passwords are read from `yearbook.json`.

```pwsh
./1-DeployDatabase.ps1 -DatabaseBackupFile "path/to/previous-year-dump.sql" -Dump PreviousYear
./1-DeployDatabase.ps1 -DatabaseBackupFile "path/to/current-year-dump.sql" -Dump CurrentYear
```

### 3. Cleanup Database

Filters guild members to keep only the relevant server.

```pwsh
./2-CleanupDatabase.ps1
```

### 4. Generate Most Active (All Users)

Queries both database dumps to calculate activity for all users this year and outputs a JSON file to `output/`.

```pwsh
./3-GenerateMostActiveAll.ps1
```

### 5. Generate Most Active (New Users)

Queries the current year dump to calculate activity for users who joined during the year and outputs a JSON file to `output/`.

```pwsh
./4-GenerateMostActiveNew.ps1
```

### 6. Export All Users to CSV

Converts the all-users JSON to a ranked CSV file in `output/`.

```pwsh
./5-ExportAllToCsv.ps1
```

### 7. Export New Users to CSV

Converts the new-users JSON to a ranked CSV file in `output/`.

```pwsh
./6-ExportNewToCsv.ps1
```

### 8. Generate Recap Images

Generates personalized recap images for each user using a background image, default avatar, and a directory of user avatars. Images are saved to `output/generated-recaps/`.

```pwsh
./7-GenerateRecapImages.ps1 -BackgroundImagePath "path/to/background.jpg" -DefaultAvatarPath "path/to/default.png" -AvatarDirectory "path/to/avatars" -Count 100
```

### 9. Generate Recap SQL

Converts the generated recap images into SQL INSERT statements for importing into `taylorbot-postgres`. Scripts are saved to `output/generated-sql/`.

```pwsh
./8-GenerateRecapSql.ps1
```

## Historical Notes

- **2018**: Only the new-users JSON was generated (no all-users, no CSVs). There was no previous-year dump; the single dump was used as the current year. The `first_joined_at` column was stored as `bigint` (epoch milliseconds); the query auto-detects this and converts using `to_timestamp()`.
- **2018–2020**: The new-users JSON was sorted by `minutesPerDay` only; use `./4-GenerateMostActiveNew.ps1 -SortByMinutes` to reproduce.
- **2019**: The `users.users` table did not have a `username` column; the query auto-detects this and falls back to joining on `users.usernames` (history table with `MAX(changed_at)`).
- **2019–2020**: Both dumps (2018 and 2019 previous-year) are `pg_dumpall` (cluster dumps). The deploy script uses `-LocalRestore` (via `docker exec`) to avoid password issues from `ALTER ROLE` in cluster dumps.
- **2019–2022**: The all-users JSON included all users without the `messageCountThisYear > 10` filter; use `./3-GenerateMostActiveAll.ps1 -IncludeAllUsers` to reproduce. No CSVs were generated for 2019–2021.
- **2020–2023**: The all-users query joined on `users.usernames` (history table with `MAX(changed_at)`) to get usernames. This was replaced with a simpler `JOIN users.users` which has the current username directly. The old join excluded a small number of guild members (~25) that had no username history entry, all with negligible activity.
- **2022–2023**: Both CSVs were ranked by `minutesPerDay`. Use `./5-ExportAllToCsv.ps1 -SortByMinutes` and `./6-ExportNewToCsv.ps1 -SortByMinutes` to reproduce.
- **2024**: New-users CSV was ranked by `minutesPerDay`. Use `./6-ExportNewToCsv.ps1 -SortByMinutes` to reproduce.
- **2025+**: CSVs are ranked by a weighted activity score `(messagesPerDay + minutesPerDay * 1.5) / 2`.
