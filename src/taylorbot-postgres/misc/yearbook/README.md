# Yearbook

Scripts for generating yearly activity stats for a Discord server. Uses database dumps from two consecutive years of [taylorbot-postgres](../../) to calculate activity metrics, rank users, and generate personalized recap images. For example, in our main server, dumps are taken on server anniversary (November 22) every year to run these scripts.

## Setup

Make sure you have [.NET](https://dotnet.microsoft.com/) and [Docker](https://www.docker.com/) installed.

## Steps

### 1. Deploy Databases

Deploy local PostgreSQL instances and restore database backup dumps for both the previous and current year.

```pwsh
./1-DeployDatabase.ps1 -DatabaseBackupFile "path/to/previous-year-dump.sql"
./1-DeployDatabase.ps1 -DatabaseBackupFile "path/to/current-year-dump.sql"
```

### 2. Configure

Create a copy of [`template.yearbook.json`](template.yearbook.json) and rename it to `yearbook.json`. Set `year` to the yearbook year, `dumpMonth` and `dumpDay` to the date the dumps were taken (e.g. `11` and `22` for the November 22 server anniversary), and configure database connection details for both the previous and current year dumps deployed in the previous step.

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
