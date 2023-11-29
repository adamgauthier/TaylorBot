using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Imgur.Commands;

public class ImgurSlashCommand(IRateLimiter rateLimiter, ImgurClient imgurClient) : ISlashCommand<ImgurSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("imgur");

    public record Options(ParsedOptionalAttachment file, ParsedOptionalString link);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                if ((options.file.Value == null && options.link.Value == null) ||
                    (options.file.Value != null && options.link.Value != null))
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        """
                        Please use **exactly one of the parameters** with the command 😊
                        - **file** to upload from your device
                        - **link** to simply use an existing URL to a photo
                        """));
                }

                var url = options.file.Value?.url ?? options.link.Value ?? throw new InvalidOperationException();

                var rateLimitResult = await rateLimiter.VerifyDailyLimitAsync(context.User, "imgur-upload");
                if (rateLimitResult != null)
                    return rateLimitResult;

                var result = await imgurClient.UploadAsync(url);

                switch (result)
                {
                    case UploadSuccess uploaded:
                        return new EmbedResult(new EmbedBuilder()
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithDescription(
                            $"""
                            Successfully uploaded your image to Imgur 😊
                            {uploaded.Url}
                            """)
                            .WithImageUrl(uploaded.Url)
                        .Build());

                    case FileTypeInvalid _:
                        return new EmbedResult(EmbedFactory.CreateError(
                            """
                            Sorry, Imgur does not accept this file type. Are you sure it's a photo? 🤔
                            Try another one! 😕
                            """));

                    case GenericImgurError _:
                        return new EmbedResult(EmbedFactory.CreateError(
                            """
                            Imgur returned an unexpected error 😢
                            The site might be down. Try again later!
                            """));

                    default:
                        throw new InvalidOperationException(result.GetType().Name);
                };
            }
        ));
    }
}
