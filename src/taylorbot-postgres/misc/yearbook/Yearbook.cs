#:package Npgsql@10.0.1
#:package CsvHelper@33.1.0
#:package SixLabors.ImageSharp@3.1.12
#:package SixLabors.ImageSharp.Drawing@2.1.7

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using CsvHelper;
using CsvHelper.Configuration;
using Npgsql;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

var mode = args.Length > 0 ? args[0] : throw new ArgumentException("Missing mode argument. Use: cleanup, query-all-activity, query-new-activity, csv-all, csv-new, generate-images, or recap");

switch (mode)
{
    case "cleanup":
        {
            var config = JsonSerializer.Deserialize(File.ReadAllText("yearbook.json"), YearbookJsonContext.Default.YearbookConfig)!;
            await using NpgsqlConnection connPrev = new(config.dumpPreviousYear.ToConnectionString());
            await using NpgsqlConnection connCurr = new(config.dumpCurrentYear.ToConnectionString());
            await connPrev.OpenAsync();
            await connCurr.OpenAsync();

            await using NpgsqlCommand cmdPrev = new("DELETE FROM guilds.guild_members WHERE guild_id <> '115332333745340416'", connPrev);
            await cmdPrev.ExecuteNonQueryAsync();

            await using NpgsqlCommand cmdCurr = new("DELETE FROM guilds.guild_members WHERE guild_id <> '115332333745340416'", connCurr);
            await cmdCurr.ExecuteNonQueryAsync();
            break;
        }
    case "query-all-activity":
        {
            var config = JsonSerializer.Deserialize(File.ReadAllText("yearbook.json"), YearbookJsonContext.Default.YearbookConfig)!;
            var year = config.year;
            await using NpgsqlConnection connPrev = new(config.dumpPreviousYear.ToConnectionString());
            await using NpgsqlConnection connCurr = new(config.dumpCurrentYear.ToConnectionString());
            await connPrev.OpenAsync();
            await connCurr.OpenAsync();

            List<PreviousYearRow> rowsPrev = [];
            await using (NpgsqlCommand cmd = new("""
            SELECT gm.user_id, gm.message_count, gm.minute_count
            FROM guilds.guild_members AS gm
            WHERE guild_id = '115332333745340416'
            """, connPrev))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    rowsPrev.Add(new(reader.GetString(0), reader.GetInt64(1), reader.GetInt64(2)));
                }
            }
            Console.WriteLine($"Got {rowsPrev.Count} rows from previous year dump");

            List<CurrentYearRow> rowsCurr = [];
            await using (NpgsqlCommand cmd = new("""
            SELECT gm.user_id, gm.message_count, gm.first_joined_at, gm.minute_count, u.username
            FROM guilds.guild_members AS gm
            JOIN users.users AS u ON gm.user_id = u.user_id
            WHERE gm.guild_id = '115332333745340416'
            """, connCurr))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    rowsCurr.Add(new(
                        reader.GetString(0),
                        reader.GetInt64(1),
                        reader.IsDBNull(2) ? null : GetUtcDateTime(reader, 2),
                        reader.GetInt64(3),
                        reader.GetString(4)
                    ));
                }
            }
            Console.WriteLine($"Got {rowsCurr.Count} rows from current year dump");

            DateTimeOffset referenceEnd = new(year, config.dumpMonth, config.dumpDay, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset referenceStart = new(year - 1, config.dumpMonth, config.dumpDay, 0, 0, 0, TimeSpan.Zero);

            var users = rowsCurr.Select(r =>
            {
                var userPrev = rowsPrev.Find(u => u.UserId == r.UserId);
                var messageCount = userPrev != null ? r.MessageCount - userPrev.MessageCount : r.MessageCount;
                var minuteCount = userPrev != null ? r.MinuteCount - userPrev.MinuteCount : r.MinuteCount;
                var diffFrom = userPrev != null ? referenceStart : (r.FirstJoinedAt.HasValue ? new DateTimeOffset(r.FirstJoinedAt.Value, TimeSpan.Zero) : referenceEnd);
                var daysDiff = (int)(referenceEnd - diffFrom).TotalDays;
                if (daysDiff == 0) daysDiff = 1;

                return new AllActivityUser(
                    r.UserId,
                    r.Username,
                    messageCount,
                    minuteCount,
                    r.FirstJoinedAt.HasValue ? new DateTimeOffset(r.FirstJoinedAt.Value, TimeSpan.Zero).ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : null,
                    r.FirstJoinedAt.HasValue ? Constants.FormatDateWithOrdinal(r.FirstJoinedAt.Value) : null,
                    (double)messageCount / daysDiff,
                    (double)minuteCount / daysDiff
                );
            })
            .Where(u => u.messageCountThisYear > 10)
            .OrderByDescending(u => u.minutesPerDay)
            .ToList();

            Directory.CreateDirectory("output");
            var outputFile = Path.Combine("output", $"most_active_users_{year}.json");
            File.WriteAllText(outputFile, JsonSerializer.Serialize(users, YearbookJsonContext.Default.ListAllActivityUser));
            Console.WriteLine($"Wrote {users.Count} users to {outputFile}");
            break;
        }
    case "query-new-activity":
        {
            var config = JsonSerializer.Deserialize(File.ReadAllText("yearbook.json"), YearbookJsonContext.Default.YearbookConfig)!;
            var year = config.year;
            await using NpgsqlConnection connCurr = new(config.dumpCurrentYear.ToConnectionString());
            await connCurr.OpenAsync();

            DateTimeOffset joinedAfter = new DateTimeOffset(year, config.dumpMonth, config.dumpDay, 0, 0, 0, TimeSpan.Zero).AddMonths(-13);
            DateTimeOffset joinedBefore = new DateTimeOffset(year, config.dumpMonth, config.dumpDay, 0, 0, 0, TimeSpan.Zero).AddMonths(-1);
            DateTimeOffset referenceEnd = new(year, config.dumpMonth, config.dumpDay, 0, 0, 0, TimeSpan.Zero);

            List<CurrentYearRow> rows = [];
            await using (NpgsqlCommand cmd = new("""
            SELECT gm.user_id, gm.message_count, gm.first_joined_at, gm.minute_count, u.username
            FROM guilds.guild_members AS gm
            INNER JOIN users.users AS u ON gm.user_id = u.user_id
            WHERE gm.guild_id = '115332333745340416'
                AND gm.first_joined_at >= @joinedAfter
                AND gm.first_joined_at <= @joinedBefore
            """, connCurr))
            {
                cmd.Parameters.AddWithValue("joinedAfter", joinedAfter.UtcDateTime);
                cmd.Parameters.AddWithValue("joinedBefore", joinedBefore.UtcDateTime);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rows.Add(new(
                        reader.GetString(0),
                        reader.GetInt64(1),
                        reader.IsDBNull(2) ? null : GetUtcDateTime(reader, 2),
                        reader.GetInt64(3),
                        reader.GetString(4)
                    ));
                }
            }
            Console.WriteLine($"Got {rows.Count} in rows");

            var users = rows.Select(r =>
            {
                var joinedAt = r.FirstJoinedAt.HasValue ? new DateTimeOffset(r.FirstJoinedAt.Value, TimeSpan.Zero) : referenceEnd;
                var daysDiff = (int)(referenceEnd - joinedAt).TotalDays;
                if (daysDiff == 0) daysDiff = 1;

                return new NewActivityUser(
                    r.UserId,
                    r.Username,
                    r.MessageCount,
                    r.MinuteCount,
                    r.FirstJoinedAt.HasValue ? new DateTimeOffset(r.FirstJoinedAt.Value, TimeSpan.Zero).ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : null,
                    r.FirstJoinedAt.HasValue ? Constants.FormatDateWithOrdinal(r.FirstJoinedAt.Value) : null,
                    (double)r.MessageCount / daysDiff,
                    (double)r.MinuteCount / daysDiff
                );
            })
            .OrderByDescending(u => u.minutesPerDay + u.messagesPerDay)
            .ToList();

            Directory.CreateDirectory("output");
            var outputFile = Path.Combine("output", $"most_active_new_users_{year}.json");
            File.WriteAllText(outputFile, JsonSerializer.Serialize(users, YearbookJsonContext.Default.ListNewActivityUser));
            Console.WriteLine($"Wrote {users.Count} users to {outputFile}");
            break;
        }
    case "csv-all":
        {
            var config = JsonSerializer.Deserialize(File.ReadAllText("yearbook.json"), YearbookJsonContext.Default.YearbookConfig)!;
            var jsonFilePath = Path.Combine("output", $"most_active_users_{config.year}.json");
            var csvFilePath = Path.Combine("output", Path.ChangeExtension(Path.GetFileName(jsonFilePath), ".csv"));

            var jsonText = File.ReadAllText(jsonFilePath);
            var users = JsonSerializer.Deserialize(jsonText, YearbookJsonContext.Default.ListCsvAllUser) ?? throw new InvalidOperationException();

            using (StreamWriter writer = new(csvFilePath))
            using (CsvWriter csv = new(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(
                    users
                    .Where(u =>
                        !Constants.BotUserIds.Contains(u.user_id)
                        && u.messagesPerDay.HasValue
                        && u.minutesPerDay.HasValue
                        && u.messageCountThisYear.HasValue
                        && u.minuteCountThisYear.HasValue
                        && u.messageCountThisYear > 10)
                    .Select(u => u with { activityScore = (u.messageCountThisYear!.Value + u.minuteCountThisYear!.Value * 1.5) / 2 })
                    .OrderByDescending(u => u.activityScore)
                    .Select((u, i) => u with { serverRank = i + 1 }));
            }

            Console.WriteLine($"CSV file '{csvFilePath}' has been created successfully.");
            break;
        }
    case "csv-new":
        {
            var config = JsonSerializer.Deserialize(File.ReadAllText("yearbook.json"), YearbookJsonContext.Default.YearbookConfig)!;
            var jsonFilePath = Path.Combine("output", $"most_active_new_users_{config.year}.json");
            var csvFilePath = Path.Combine("output", Path.ChangeExtension(Path.GetFileName(jsonFilePath), ".csv"));

            var jsonText = File.ReadAllText(jsonFilePath);
            var users = JsonSerializer.Deserialize(jsonText, YearbookJsonContext.Default.ListCsvNewUser) ?? throw new InvalidOperationException();

            using (StreamWriter writer = new(csvFilePath))
            using (CsvWriter csv = new(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(users.Where(u =>
                    u.first_joined_at != null
                    && u.messagesPerDay.HasValue
                    && u.minutesPerDay.HasValue
                    && u.message_count.HasValue
                    && u.minute_count.HasValue
                    && u.message_count > 10
                    && u.minute_count > 10
                    && u.minutesPerDay >= 1)
                    .Select(u => u with { perDayActivityScore = (u.messagesPerDay!.Value + u.minutesPerDay!.Value * 1.5) / 2 })
                    .OrderByDescending(u => u.perDayActivityScore)
                    .Take(300)
                    .Select((u, i) => u with { perDayActivityRank = i + 1 }));
            }

            Console.WriteLine($"CSV file '{csvFilePath}' has been created successfully.");
            break;
        }
    case "generate-images":
        {
            var config = JsonSerializer.Deserialize(File.ReadAllText("yearbook.json"), YearbookJsonContext.Default.YearbookConfig)!;
            var backgroundPath = args.Length > 1 ? args[1] : throw new ArgumentException("Missing background image path argument");
            var defaultAvatarPath = args.Length > 2 ? args[2] : throw new ArgumentException("Missing default avatar image path argument");
            var avatarDir = args.Length > 3 ? args[3] : throw new ArgumentException("Missing avatar directory argument");
            var outputDir = args.Length > 4 ? args[4] : throw new ArgumentException("Missing output directory argument");
            var takeCount = args.Length > 5 ? int.Parse(args[5]) : throw new ArgumentException("Missing take count argument");
            Directory.CreateDirectory(outputDir);

            using var background = Image.Load<Rgba32>(backgroundPath);

            var avatarLookup = Directory.GetFiles(avatarDir)
                .ToDictionary(f => Path.GetFileName(f).Split('-')[0], f => f);

            var csvPath = Path.Combine("output", $"most_active_users_{config.year}.csv");
            using var csvReader = new StreamReader(csvPath);
            using CsvReader csv = new(csvReader, new CsvConfiguration(CultureInfo.InvariantCulture));
            var users = csv.GetRecords<RecapUser>().Take(takeCount).ToList();

            var headerFont = SystemFonts.CreateFont("Helvetica", 160, FontStyle.Bold);
            var subHeaderFont = SystemFonts.CreateFont("Helvetica", 100, FontStyle.Bold);
            var textColor = Color.White;
            var outlineColor = Color.ParseHex("#bb5d07");

            var totalSw = System.Diagnostics.Stopwatch.StartNew();
            var completed = 0;

            await Parallel.ForEachAsync(users, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, async (user, ct) =>
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();

                if (!avatarLookup.TryGetValue(user.user_id, out var avatarPath))
                {
                    avatarPath = defaultAvatarPath;
                }

                using var avatarImage = Image.Load<Rgba32>(avatarPath);
                avatarImage.Mutate(x => x.Resize(1024, 1024));

                using var bgClone = background.Clone();
                using Image<Rgba32> recapImage = new(2160, 3840);
                recapImage.Mutate(ctx =>
                {
                    ctx.DrawImage(bgClone, Point.Empty, 1);

                    var avatarX = (recapImage.Width - avatarImage.Width) / 2;
                    var avatarY = (recapImage.Height - avatarImage.Height) / 6;
                    ctx.DrawImage(avatarImage, new Point(avatarX, avatarY), 1);

                    void DrawCenter(string text, float yPosition, Font font)
                    {
                        DrawOutlinedText(ctx, text, font, new PointF(recapImage.Width / 2, yPosition), textColor, outlineColor);
                    }

                    var usernamePosition = avatarY + avatarImage.Height + 110;
                    DrawCenter($"@{user.username}", usernamePosition, headerFont);

                    var rankPosition = usernamePosition + 160;
                    DrawCenter($"#{user.serverRank.ToString("#,0")}", rankPosition, headerFont);

                    var joinedPosition = 2050;
                    DrawCenter("Joined", joinedPosition, subHeaderFont);
                    var joinedTextPosition = joinedPosition + 125;
                    DrawCenter(user.first_joined_at_formatted, joinedTextPosition, subHeaderFont);

                    var messagesPosition = joinedTextPosition + 300;
                    DrawCenter("Messages This Year", messagesPosition, subHeaderFont);
                    var messageCountPosition = messagesPosition + 125;
                    DrawCenter(user.messageCountThisYear.ToString("#,0"), messageCountPosition, subHeaderFont);
                    var messagePerDayPosition = messageCountPosition + 125;
                    DrawCenter($"~{Math.Round(user.messagesPerDay).ToString("#,0")} per day", messagePerDayPosition, subHeaderFont);

                    var minutesPosition = messagePerDayPosition + 300;
                    DrawCenter("Minutes This Year", minutesPosition, subHeaderFont);
                    var minuteCountPosition = minutesPosition + 125;
                    DrawCenter(user.minuteCountThisYear.ToString("#,0"), minuteCountPosition, subHeaderFont);
                    var minutePerDayPosition = minuteCountPosition + 125;
                    DrawCenter($"~{Math.Round(user.minutesPerDay).ToString("#,0")} per day", minutePerDayPosition, subHeaderFont);
                });

                var outputPath = Path.Combine(outputDir, $"{user.user_id}.jpg");
                await recapImage.SaveAsJpegAsync(outputPath, ct);
                var done = Interlocked.Increment(ref completed);
                var pct = (double)done / users.Count * 100;
                Console.WriteLine($"[{done}/{users.Count} {pct:F1}%] Generated recap for {user.username} (#{user.serverRank}) using {Path.GetFileName(avatarPath)} in {sw.Elapsed.TotalSeconds:F1}s");
            });

            Console.WriteLine($"Done generating {users.Count} recap images in '{outputDir}' ({totalSw.Elapsed.TotalSeconds:F1}s total)");
            break;
        }
    case "recap":
        {
            var config = JsonSerializer.Deserialize(File.ReadAllText("yearbook.json"), YearbookJsonContext.Default.YearbookConfig)!;
            var directory = args.Length > 1 ? args[1] : throw new ArgumentException("Missing directory argument for recap mode");
            var outputDirectory = args.Length > 2 ? args[2] : throw new ArgumentException("Missing output directory argument for recap mode");
            Directory.CreateDirectory(outputDirectory);

            var jpgFiles = Directory.GetFiles(directory, "*.jpg");
            var chunks = jpgFiles.Chunk(100);

            var i = 1;
            foreach (var chunk in chunks)
            {
                var outputFile = Path.Combine(outputDirectory, $"{i}-insert-recaps.sql");
                using StreamWriter writer = new(outputFile);

                foreach (var file in chunk)
                {
                    var base64Content = Convert.ToBase64String(await File.ReadAllBytesAsync(file));
                    await writer.WriteLineAsync($"INSERT INTO configuration.application_info (info_key, info_value) VALUES ('recap_{config.year}_{Path.GetFileName(file)}.base64', '{base64Content}');");
                }

                Console.WriteLine($"Recap script output file '{outputFile}' has been created successfully.");
                i++;
            }
            break;
        }
    default:
        throw new ArgumentException($"Unknown mode '{mode}'. Use: cleanup, query-all-activity, query-new-activity, csv-all, csv-new, generate-images, or recap");
}

static void DrawOutlinedText(IImageProcessingContext ctx, string text, Font font, PointF position, Color fillColor, Color outlineColor)
{
    RichTextOptions options = new(font) { Origin = position, TextAlignment = TextAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

    var outlineThickness = 12;
    for (var dx = -outlineThickness; dx <= outlineThickness; dx++)
    {
        for (var dy = -outlineThickness; dy <= outlineThickness; dy++)
        {
            if (dx == 0 && dy == 0)
                continue;

            RichTextOptions outlineOptions = new(options)
            {
                Origin = new(options.Origin.X + dx, options.Origin.Y + dy)
            };
            ctx.DrawText(outlineOptions, text, outlineColor);
        }
    }
    ctx.DrawText(options, text, fillColor);
}

static DateTime GetUtcDateTime(System.Data.Common.DbDataReader reader, int ordinal)
{
    var dt = reader.GetDateTime(ordinal);
    return dt.Kind == DateTimeKind.Utc ? dt : throw new InvalidOperationException($"Expected UTC DateTime but got {dt.Kind}");
}

record PreviousYearRow(string UserId, long MessageCount, long MinuteCount);
record CurrentYearRow(string UserId, long MessageCount, DateTime? FirstJoinedAt, long MinuteCount, string Username);
record AllActivityUser(string user_id, string username, long messageCountThisYear, long minuteCountThisYear, string? first_joined_at, string? first_joined_at_formatted, double messagesPerDay, double minutesPerDay);
record NewActivityUser(string user_id, string username, long message_count, long minute_count, string? first_joined_at, string? first_joined_at_formatted, double messagesPerDay, double minutesPerDay);
record CsvAllUser(string user_id, string username, int? messageCountThisYear, int? minuteCountThisYear, string? first_joined_at, string first_joined_at_formatted, double? messagesPerDay, double? minutesPerDay, double? activityScore, int serverRank);
record CsvNewUser(string user_id, string username, int? message_count, int? minute_count, string? first_joined_at, string first_joined_at_formatted, double? messagesPerDay, double? minutesPerDay, double? perDayActivityScore, int perDayActivityRank);
record RecapUser(string user_id, string username, int messageCountThisYear, int minuteCountThisYear, string first_joined_at_formatted, double messagesPerDay, double minutesPerDay, int serverRank);

static class Constants
{
    public static readonly string[] BotUserIds = ["119572982178906114", "349977940198555660", "713349440139821107", "120235985865801728", "622420961982939176"];

    public static string FormatDateWithOrdinal(DateTime date)
    {
        var day = date.Day;
        var suffix = day switch
        {
            1 or 21 or 31 => "st",
            2 or 22 => "nd",
            3 or 23 => "rd",
            _ => "th",
        };
        return $"{date:MMMM} {day}{suffix} {date:yyyy}";
    }
}

record YearbookConfig(int year, int dumpMonth, int dumpDay, DbConfig dumpPreviousYear, DbConfig dumpCurrentYear);

record DbConfig(string host, int port, string database, string user, string password)
{
    public string ToConnectionString() => $"Host={host};Port={port};Database={database};Username={user};Password={password};Command Timeout=300";
}

[JsonSerializable(typeof(YearbookConfig))]
[JsonSerializable(typeof(List<AllActivityUser>))]
[JsonSerializable(typeof(List<NewActivityUser>))]
[JsonSerializable(typeof(List<CsvAllUser>))]
[JsonSerializable(typeof(List<CsvNewUser>))]
partial class YearbookJsonContext : JsonSerializerContext { }
