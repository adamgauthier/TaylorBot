using CsvHelper;
using Dapper;
using Discord;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Globalization;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Http;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.User;
using ImageSharp = SixLabors.ImageSharp;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events;

public interface IMemberActivityRepository
{
    Task<byte[]> GetRecapBackgroundImageAsync();

    Task<MemberActivity?> GetYearActivityForUserAsync(DiscordUser user);
}

public record MemberActivity(string user_id, string username, int messageCountThisYear, int minuteCountThisYear, string first_joined_at_formatted, float messagesPerDay, float minutesPerDay, int serverRank);

public class PostgresMemberActivityRepository(PostgresConnectionFactory postgresConnectionFactory, IMemoryCache memoryCache) : IMemberActivityRepository
{
    public async Task<byte[]> GetRecapBackgroundImageAsync()
    {
        var backgroundImageBytes = await memoryCache.GetOrCreateAsync(
            "recap_2024_bg.jpg.base64",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);

                await using var connection = postgresConnectionFactory.CreateConnection();

                var base64 = await connection.QuerySingleAsync<string>(
                    """
                    SELECT info_value FROM configuration.application_info WHERE info_key = 'recap_2024_bg.jpg.base64';
                    """);

                return Convert.FromBase64String(base64);
            });
        ArgumentNullException.ThrowIfNull(backgroundImageBytes);
        return backgroundImageBytes;
    }

    public async Task<MemberActivity?> GetYearActivityForUserAsync(DiscordUser user)
    {
        var records = await memoryCache.GetOrCreateAsync(
            "most_active_users2024.csv",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                await using var connection = postgresConnectionFactory.CreateConnection();

                var csvContent = await connection.QuerySingleAsync<string>(
                    """
                    SELECT info_value FROM configuration.application_info WHERE info_key = 'most_active_users2024.csv';
                    """);

                using var reader = new StringReader(csvContent);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                return csv.GetRecords<MemberActivity>().ToList();
            });
        ArgumentNullException.ThrowIfNull(records);

        return records.FirstOrDefault(r => r.user_id == user.Id);
    }
}

public class RecapSlashCommand(
    ILogger<RecapSlashCommand> logger,
    IRateLimiter rateLimiter,
    IMemberActivityRepository memberActivityRepository,
    IHttpClientFactory clientFactory,
    IMemoryCache memoryCache
    ) : ISlashCommand<NoOptions>
{
    public static string CommandName => "recap";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions _)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var rateLimitResult = await rateLimiter.VerifyDailyLimitAsync(context.User, "generate-recap");
                if (rateLimitResult != null)
                    return rateLimitResult;

                var user = context.User;
                var activity = await memberActivityRepository.GetYearActivityForUserAsync(user);

                if (activity == null)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        Sorry, it looks like you were not part of the 10,000 most active members of 2024 😕
                        Maybe next year! 🙏
                        """));
                }

                var imageBytes = await memoryCache.GetOrCreateAsync(
                    $"recap-{user.Id}",
                    factory: async entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromHours(1);
                        return await GenerateRecapImageAsync(logger, clientFactory, user, activity);
                    });
                ArgumentNullException.ThrowIfNull(imageBytes);

                const string filename = "recap.png";
                var imageStream = new MemoryStream(imageBytes);

                var embed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("Taycord Recap 2024 ✨")
                    .WithDescription(
                        """
                        Here's your 2024 recap **designed by Adam & FullyCustom** 🖌️
                        Submit your [Yearbook](https://discord.com/channels/115332333745340416/123150327456333824/1312535164714483793) signature with **/signature** if you haven't 😊
                        """)
                    .WithImageUrl($"attachment://{filename}");

                return new MessageResult(new([embed.Build()], Attachments: [new Attachment(imageStream, filename)]));
            },
            Preconditions: [new InGuildPrecondition()]
        ));
    }


    private static readonly Font HeaderFont = SystemFonts.CreateFont("Helvetica", 160, FontStyle.Bold);
    private static readonly Font SubHeaderFont = SystemFonts.CreateFont("Helvetica", 100, FontStyle.Bold);

    private static readonly ImageSharp.Color TextColor = ImageSharp.Color.White;
    private static readonly ImageSharp.Color OutlineColor = ImageSharp.Color.ParseHex("#773091");

    private async Task<byte[]> GenerateRecapImageAsync(ILogger<RecapSlashCommand> logger, IHttpClientFactory clientFactory, DiscordUser user, MemberActivity activity)
    {
        using var client = clientFactory.CreateClient();
        using var response = await client.GetAsync(user.GetGuildAvatarUrlOrDefault(size: 1024));
        await response.EnsureSuccessAsync(logger);

        using var avatarStream = await response.Content.ReadAsStreamAsync();
        using var avatarImage = ImageSharp.Image.Load<Rgba32>(avatarStream);
        avatarImage.Mutate(x => x.Resize(1024, 1024));

        var recapBackground = await memberActivityRepository.GetRecapBackgroundImageAsync();

        var recapImage = new Image<Rgba32>(2160, 3840);
        recapImage.Mutate(ctx =>
        {
            using var background = ImageSharp.Image.Load<Rgba32>(recapBackground);
            ctx.DrawImage(background, Point.Empty, 1);

            var avatarX = (recapImage.Width - avatarImage.Width) / 2;
            var avatarY = (recapImage.Height - avatarImage.Height) / 6;
            ctx.DrawImage(avatarImage, new Point(avatarX, avatarY), 1);

            void DrawCenter(string text, float yPosition, Font font)
            {
                DrawOutlinedText(ctx, text, font, new(recapImage.Width / 2, yPosition), TextColor, OutlineColor);
            }

            var usernamePosition = avatarY + avatarImage.Height + 110;
            DrawCenter($"@{user.Username}", usernamePosition, HeaderFont);

            var rankPosition = usernamePosition + 160;
            DrawCenter($"#{activity.serverRank.ToString(TaylorBotFormats.Readable)}", rankPosition, HeaderFont);

            var joinedPosition = 2050;
            DrawCenter("Joined", joinedPosition, SubHeaderFont);
            var joinedTextPosition = joinedPosition + 125;
            DrawCenter(activity.first_joined_at_formatted, joinedTextPosition, SubHeaderFont);

            var messagesPosition = joinedTextPosition + 300;
            DrawCenter("Messages This Year", messagesPosition, SubHeaderFont);
            var messageCountPosition = messagesPosition + 125;
            DrawCenter($"{activity.messageCountThisYear.ToString(TaylorBotFormats.Readable)}", messageCountPosition, SubHeaderFont);
            var messagePerDayPosition = messageCountPosition + 125;
            DrawCenter($"~{Math.Round(activity.messagesPerDay).ToString(TaylorBotFormats.Readable)} per day", messagePerDayPosition, SubHeaderFont);

            var minutesPosition = messagePerDayPosition + 300;
            DrawCenter("Minutes This Year", minutesPosition, SubHeaderFont);
            var minuteCountPosition = minutesPosition + 125;
            DrawCenter($"{activity.minuteCountThisYear.ToString(TaylorBotFormats.Readable)}", minuteCountPosition, SubHeaderFont);
            var minutePerDayPosition = minuteCountPosition + 125;
            DrawCenter($"~{Math.Round(activity.minutesPerDay).ToString(TaylorBotFormats.Readable)} per day", minutePerDayPosition, SubHeaderFont);
        });

        var imageStream = new MemoryStream();
        await recapImage.SaveAsJpegAsync(imageStream);
        imageStream.Seek(0, SeekOrigin.Begin);
        return imageStream.ToArray();
    }

    private static void DrawOutlinedText(IImageProcessingContext ctx, string text, Font font, ImageSharp.PointF position, ImageSharp.Color fillColor, ImageSharp.Color outlineColor)
    {
        RichTextOptions options = new(font) { Origin = position, TextAlignment = TextAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

        int outlineThickness = 12;
        for (int dx = -outlineThickness; dx <= outlineThickness; dx++)
        {
            for (int dy = -outlineThickness; dy <= outlineThickness; dy++)
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
}
