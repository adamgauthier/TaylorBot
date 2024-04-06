using Discord;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;

public interface IObsessionRepository
{
    ValueTask<string?> GetObsessionAsync(DiscordUser user);
    ValueTask SetObsessionAsync(DiscordUser user, string obsession);
    ValueTask ClearObsessionAsync(DiscordUser user);
}

public class FavoriteObsessionShowSlashCommand(IObsessionRepository obsessionRepository) : ISlashCommand<FavoriteObsessionShowSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("favorite obsession show");

    public record Options(ParsedFetchedUserOrAuthor user);

    public static Embed BuildDisplayEmbed(DiscordUser user, string favoriteObsession)
    {
        return new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithUserAsAuthor(user)
            .WithTitle("Obsession ❤️")
            .WithImageUrl(favoriteObsession)
            .Build();
    }

    public Command Show(IUser user, RunContext? context = null) => new(
        new(Info.Name),
        async () =>
        {
            var favoriteObsession = await obsessionRepository.GetObsessionAsync(new(user));

            if (favoriteObsession != null)
            {
                if (!Uri.TryCreate(favoriteObsession, UriKind.Absolute, out var url) ||
                    !url.IsWellFormedOriginalString() || url.Scheme is not ("http" or "https"))
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        The obsession for this user is not a valid URL to a photo! 😕
                        They need to use {context?.MentionCommand("favorite obsession set") ?? "</favorite obsession set:1169468169140838502>"} to update it.
                        """));
                }

                Embed embed = BuildDisplayEmbed(new(user), favoriteObsession);
                return new EmbedResult(embed);
            }
            else
            {
                return new EmbedResult(EmbedFactory.CreateError(
                    $"""
                    {user.Mention}'s obsession is not set. 🚫
                    They need to use {context?.MentionCommand("favorite obsession set") ?? "</favorite obsession set:1169468169140838502>"} to set it first.
                    """));
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Show(options.user.User, context));
    }
}

public class FavoriteObsessionSetSlashCommand(IObsessionRepository obsessionRepository) : ISlashCommand<FavoriteObsessionSetSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("favorite obsession set");

    public record Options(ParsedString obsession);

    public Command Set(DiscordUser user, string favoriteObsession, RunContext? context = null) => new(
        new(Info.Name),
        async () =>
        {
            if (!Uri.TryCreate(favoriteObsession, UriKind.Absolute, out var url) ||
                !url.IsWellFormedOriginalString() || url.Scheme is not ("http" or "https"))
            {
                return new EmbedResult(EmbedFactory.CreateError("The obsession you specified is not a valid URL to a photo. 😕"));
            }

            if (context != null)
            {
                var embed = FavoriteObsessionShowSlashCommand.BuildDisplayEmbed(user, favoriteObsession);
                return MessageResult.CreatePrompt(
                    new([embed, EmbedFactory.CreateWarning("Are you sure you want to set your obsession to the above?")]),
                    confirm: async () => new(await SetAsync(favoriteObsession))
                );
            }
            else
            {
                return new EmbedResult(await SetAsync(favoriteObsession));
            }

            async ValueTask<Embed> SetAsync(string favoriteObsession)
            {
                await obsessionRepository.SetObsessionAsync(user, favoriteObsession);

                return EmbedFactory.CreateSuccess(
                    $"""
                    Your obsession has been set successfully. ✅
                    Others can now use {context?.MentionCommand("favorite obsession show")} to see your obsession. ❤️
                    """);
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Set(context.User, options.obsession.Value, context));
    }
}

public class FavoriteObsessionClearSlashCommand(IObsessionRepository obsessionRepository) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("favorite obsession clear");

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await obsessionRepository.ClearObsessionAsync(context.User);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Your obsession has been cleared and is no longer visible. ✅
                    You can set it again with {context.MentionCommand("favorite obsession set")}.
                    """));
            }
        ));
    }
}
