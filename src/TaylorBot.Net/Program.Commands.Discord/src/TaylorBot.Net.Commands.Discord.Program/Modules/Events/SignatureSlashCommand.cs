using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Http;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events;

public static class AnniversaryEvent
{
    public static readonly DateTimeOffset Start = new DateTimeOffset(2025, 11, 22, 0, 0, 0, TimeSpan.Zero) - TimeSpan.FromHours(12);
    public static readonly DateTimeOffset End = new DateTimeOffset(2025, 11, 23, 0, 0, 0, TimeSpan.Zero) + TimeSpan.FromHours(12);

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
    IRateLimiter rateLimiter,
    IServerJoinedRepository serverJoinedRepository,
    InGuildPrecondition.Factory inGuild) : ISlashCommand<SignatureSlashCommand.Options>
{
    public static string CommandName => "signature";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedOptionalAttachment file, ParsedOptionalString link);

    public IList<ICommandPrecondition> BuildPreconditions() => [inGuild.Create()];

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

                var promptEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.WarningColor)
                    .WithDescription(
                        """
                        Please confirm the picture you are uploading is:
                        - Your signature either in digital form or a real photo of your writing on a piece of paper ✅
                        - Using a darker color (light colors like white/yellow will blend in with the yearbook's light background) 🖊️
                        - For real photos, the background is easy to remove (blank page, no lines, good lighting) 💡
                        - Not the same picture you've submitted in previous years 🆕
                        """)
                    .WithImageUrl(url)
                    .Build();

                return MessageResult.CreatePrompt(
                    new(promptEmbed),
                    InteractionCustomId.Create(SignatureConfirmButtonHandler.CustomIdName)
                );
            },
            Preconditions: BuildPreconditions()
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

public partial class SignatureConfirmButtonHandler(
    ILogger<SignatureConfirmButtonHandler> logger,
    SignatureSlashCommand command,
    IInteractionResponseClient responseClient,
    [FromKeyedServices("SignatureContainer")] Lazy<BlobContainerClient> signatureContainer,
    IHttpClientFactory clientFactory) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.SignatureConfirm;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        Preconditions: command.BuildPreconditions(),
        RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var promptMessage = button.Interaction.Raw.message;
        ArgumentNullException.ThrowIfNull(promptMessage);

        if (DateTimeOffset.UtcNow - promptMessage.timestamp > TimeSpan.FromDays(1))
        {
            await responseClient.EditOriginalResponseAsync(button.Interaction, EmbedFactory.CreateErrorEmbed(
                """
                Oops, looks like it's been too long since you ran this command 😵
                Please run the command again 🔃
                """));
            return;
        }

        var url = promptMessage.embeds[0].image?.url;
        ArgumentNullException.ThrowIfNull(url);

        var user = context.User;

        var fileExtension = Path.GetExtension(new Uri(url).AbsolutePath);
        var blob = signatureContainer.Value.GetBlobClient($"{user.Id}-{user.Username}{fileExtension}");

        var signatureExists = await blob.ExistsAsync();
        if (signatureExists)
        {
            await responseClient.EditOriginalResponseAsync(button.Interaction, EmbedFactory.CreateErrorEmbed(
                """
                Oops, it looks like you already uploaded your signature 😕
                If you want to update your signature, please contact Adam directly!
                """));
            return;
        }

        using var client = clientFactory.CreateClient();
        using var response = await client.GetAsync(url);

        if (!await response.VerifyStatusAsync(logger))
        {
            await responseClient.EditOriginalResponseAsync(button.Interaction, EmbedFactory.CreateErrorEmbed(
                """
                Oops, something went wrong when trying to download your signature. Try again or upload it to imgur.com beforehand 😕
                If this keeps happening, please contact Adam directly!
                """));
            return;
        }

        const int MaxSizeInBytes = 10 * 1024 * 1024; // 10 MB

        var contentLength = response.Content.Headers.ContentLength ?? 0;
        if (contentLength > MaxSizeInBytes)
        {
            LogSignatureTooLarge(contentLength);
            await responseClient.EditOriginalResponseAsync(button.Interaction, EmbedFactory.CreateErrorEmbed(
                """
                Oops, it seems like your signature file is too large. Please make sure it is under 10MB 😕
                If this keeps happening, please contact Adam directly!
                """));
            return;
        }

        using var stream = await response.Content.ReadAsStreamAsync();

        var contentType = response.Content.Headers.ContentType?.ToString();
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            await blob.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = new() { ContentType = contentType } });
        }
        else
        {
            await blob.UploadAsync(stream);
        }

        await responseClient.EditOriginalResponseAsync(button.Interaction, EmbedFactory.CreateSuccessEmbed(
            """
            Your Yearbook 2025 signature has been successfully uploaded! Thank you for your contribution to our community 💖
            Please take a moment to complete the anniversary survey as well 👀
            """));
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Signature too large to download, content length is {ContentLength}")]
    private partial void LogSignatureTooLarge(long contentLength);
}
