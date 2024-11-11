using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Http;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events;

public static class AnniversaryEvent
{
    public static readonly DateTimeOffset Start = new DateTimeOffset(2024, 11, 22, 0, 0, 0, TimeSpan.Zero) - TimeSpan.FromHours(12);
    public static readonly DateTimeOffset End = new DateTimeOffset(2024, 11, 23, 0, 0, 0, TimeSpan.Zero) + TimeSpan.FromHours(12);

    public static bool IsActive
    {
        get
        {
            var now = DateTimeOffset.UtcNow;
            return now >= Start && now <= End;
        }
    }
}

public class SignatureSlashCommand(
    ILogger<SignatureSlashCommand> logger,
    IOptionsMonitor<SignatureOptions> signatureOptions,
    IRateLimiter rateLimiter,
    IServerJoinedRepository serverJoinedRepository,
    IHttpClientFactory clientFactory
    ) : ISlashCommand<SignatureSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("signature");

    public record Options(ParsedOptionalAttachment file, ParsedOptionalString link);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var user = context.User;
                var joinedAt = await GetJoinedAtAsync(user);

                if (joinedAt > AnniversaryEvent.End + TimeSpan.FromDays(7))
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        """
                        Oops, it looks like you joined after the server anniversary so your signature can't be submitted, you will have to wait until next year 😕
                        If this is a mistake, please contact Adam directly!
                        """));
                }

                if (options.file.Value == null && options.link.Value == null ||
                    options.file.Value != null && options.link.Value != null)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        """
                        Please use **exactly one of the parameters** with the command 😊
                        - **file** to upload from your device
                        - **link** to simply use an existing URL to a photo
                        """));
                }

                var url = options.file.Value?.url ?? options.link.Value ?? throw new InvalidOperationException();

                var rateLimitResult = await rateLimiter.VerifyDailyLimitAsync(context.User, "submit-signature");
                if (rateLimitResult != null)
                    return rateLimitResult;

                return MessageResult.CreatePrompt(
                    new(EmbedFactory.CreateWarning(
                        """
                        Please confirm the picture you are uploading is:
                        - Your signature either in digital form or a real photo of your writing on a piece of paper ✅
                        - For real photos, the background is easy to remove (blank page, no lines, good lighting) 💡
                        - Not the same picture you've submitted in previous years 🆕
                        """)),
                    confirm: async () =>
                    {
                        BlobContainerClient container = new(signatureOptions.CurrentValue.BlobConnectionString, blobContainerName: "signatures2024");

                        var fileExtension = Path.GetExtension(new Uri(url).AbsolutePath);
                        var blob = container.GetBlobClient($"{user.Id}{fileExtension}");

                        var signatureExists = await blob.ExistsAsync();
                        if (signatureExists)
                        {
                            return new(EmbedFactory.CreateError(
                                """
                                Oops, it looks like you already uploaded your signature. 😕
                                If you want to update your signature, please contact Adam directly!
                                """));
                        }

                        using var client = clientFactory.CreateClient();
                        using var response = await client.GetAsync(url);

                        if (!await response.VerifyStatusAsync(logger))
                        {
                            return new(EmbedFactory.CreateError(
                                """
                                Oops, something went wrong when trying to download your signature. Try again or upload it to imgur.com beforehand 😕
                                If this keeps happening, please contact Adam directly!
                                """));
                        }

                        const int MaxSizeInBytes = 10 * 1_024 * 1_024; // 10 MB

                        var contentLength = response.Content.Headers.ContentLength ?? 0;
                        if (contentLength > MaxSizeInBytes)
                        {
                            logger.LogWarning("Signature too large to download, content length is {ContentLength}", contentLength);
                            return new(EmbedFactory.CreateError(
                                """
                                Oops, it seems like your signature file is too large. Please make sure it is under 10MB 😕
                                If this keeps happening, please contact Adam directly!
                                """));
                        }

                        using var stream = await response.Content.ReadAsStreamAsync();

                        BlobUploadOptions uploadOptions = new()
                        {
                            Tags = new Dictionary<string, string>
                            {
                                { "username", user.Username },
                            },
                        };

                        var contentType = response.Content.Headers.ContentType?.ToString();
                        if (!string.IsNullOrWhiteSpace(contentType))
                        {
                            uploadOptions.HttpHeaders = new() { ContentType = contentType };
                        }

                        await blob.UploadAsync(stream, uploadOptions);

                        return new(EmbedFactory.CreateSuccess(
                            """
                            Your Yearbook 2024 signature has been successfully uploaded! Thank you for your contribution to our community 💖
                            Please take a moment to complete the anniversary survey as well 👀
                            """));
                    }
                );
            },
            Preconditions: [new InGuildPrecondition()]
        ));
    }

    private async Task<DateTimeOffset> GetJoinedAtAsync(DiscordUser user)
    {
        ArgumentNullException.ThrowIfNull(user.MemberInfo);

        ServerJoined joined = await GetServerJoinedAsync(new(user, user.MemberInfo));

        return joined.first_joined_at ?? throw new InvalidOperationException();
    }

    private async Task<ServerJoined> GetServerJoinedAsync(DiscordMember guildUser)
    {
        var joined = await serverJoinedRepository.GetRankedJoinedAsync(guildUser);
        if (joined.first_joined_at == null && guildUser.Member.JoinedAt is not null)
        {
            await serverJoinedRepository.FixMissingJoinedDateAsync(guildUser);
            joined = await serverJoinedRepository.GetRankedJoinedAsync(guildUser);
        }
        return joined;
    }
}
