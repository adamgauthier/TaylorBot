using Discord;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;

public interface IFavoriteSongsRepository
{
    ValueTask<string?> GetFavoriteSongsAsync(DiscordUser user);
    ValueTask SetFavoriteSongsAsync(DiscordUser user, string songs);
    ValueTask ClearFavoriteSongsAsync(DiscordUser user);
}

public class FavoriteSongsShowSlashCommand(IFavoriteSongsRepository favoriteSongsRepository) : ISlashCommand<FavoriteSongsShowSlashCommand.Options>
{
    public const string PrefixCommandName = "fav";
    public const string PrefixCommandAlias1 = "favsongs";
    public const string PrefixCommandAlias2 = "favoritesongs";

    public static string CommandName => "favorite songs show";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserOrAuthor user);

    public static Embed BuildDisplayEmbed(DiscordUser user, string favoriteSongs)
    {
        return new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithUserAsAuthor(user)
            .WithTitle("Favorite Songs List 🎵")
            .WithDescription(favoriteSongs)
        .Build();
    }

    public Command Show(DiscordUser user, RunContext? context = null) => new(
        new(Info.Name, Aliases: [PrefixCommandName, PrefixCommandAlias1, PrefixCommandAlias2]),
        async () =>
        {
            var favoriteSongs = await favoriteSongsRepository.GetFavoriteSongsAsync(user);

            if (favoriteSongs != null)
            {
                Embed embed = BuildDisplayEmbed(user, favoriteSongs);
                return new EmbedResult(embed);
            }
            else
            {
                return new EmbedResult(EmbedFactory.CreateError(
                    $"""
                    {user.Mention}'s favorite songs list is not set. 🚫
                    They need to use {context?.MentionSlashCommand("favorite songs set") ?? "</favorite songs set:1169468169140838502>"} to set it first.
                    """));
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Show(options.user.User, context));
    }
}

public class FavoriteSongsSetSlashCommand(IFavoriteSongsRepository favoriteSongsRepository) : ISlashCommand<FavoriteSongsSetSlashCommand.Options>
{
    public const string PrefixCommandName = "setfav";
    public const string PrefixCommandAlias1 = "set fav";
    public const string PrefixCommandAlias2 = "setfavsongs";
    public const string PrefixCommandAlias3 = "set favsongs";

    public static string CommandName => "favorite songs set";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedString songs);

    public Command Set(DiscordUser user, string favoriteSongs, RunContext? context = null) => new(
        new(Info.Name, Aliases: [PrefixCommandName, PrefixCommandAlias1, PrefixCommandAlias2, PrefixCommandAlias3]),
        async () =>
        {
            if (context != null)
            {
                favoriteSongs = string.Join(
                    '\n',
                    favoriteSongs.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));

                var embed = FavoriteSongsShowSlashCommand.BuildDisplayEmbed(user, favoriteSongs);

                return MessageResult.CreatePrompt(
                    new([embed, EmbedFactory.CreateWarning("Are you sure you want to set your favorite songs list to the above?")]),
                    confirm: async () => new(await SetAsync(favoriteSongs))
                );
            }
            else
            {
                return new EmbedResult(await SetAsync(favoriteSongs));
            }

            async ValueTask<Embed> SetAsync(string favoriteSongs)
            {
                await favoriteSongsRepository.SetFavoriteSongsAsync(user, favoriteSongs);

                return EmbedFactory.CreateSuccess(
                    $"""
                    Your favorite songs list has been set successfully. ✅
                    Others can now use {context?.MentionSlashCommand("favorite songs show") ?? "</favorite songs show:1169468169140838502>"} to see your favorite songs. 🎵
                    """);
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Set(context.User, options.songs.Value, context));
    }
}

public class FavoriteSongsClearSlashCommand(IFavoriteSongsRepository favoriteSongsRepository) : ISlashCommand<NoOptions>
{
    public static string CommandName => "favorite songs clear";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await favoriteSongsRepository.ClearFavoriteSongsAsync(context.User);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Your favorite songs list has been cleared and is no longer be visible. ✅
                    You can set it again with {context.MentionSlashCommand("favorite songs set")}.
                    """));
            }
        ));
    }
}
