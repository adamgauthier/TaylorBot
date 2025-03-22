using Discord;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Client;
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

public class FavoriteSongsShowSlashCommand(IFavoriteSongsRepository favoriteSongsRepository, CommandMentioner mention) : ISlashCommand<FavoriteSongsShowSlashCommand.Options>
{
    public const string PrefixCommandName = "fav";
    public const string PrefixCommandAlias1 = "favsongs";
    public const string PrefixCommandAlias2 = "favoritesongs";

    public static string CommandName => "favorite songs show";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserOrAuthor user);

    public const string EmbedTitle = "Favorite Songs List 🎵";

    public static Embed BuildDisplayEmbed(DiscordUser user, string favoriteSongs)
    {
        return new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithUserAsAuthor(user)
            .WithTitle(EmbedTitle)
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
                    They need to use {mention.SlashCommand("favorite songs set", context)} to set it first.
                    """));
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Show(options.user.User, context));
    }
}

public class FavoriteSongsSetSlashCommand(IFavoriteSongsRepository favoriteSongsRepository, CommandMentioner mention) : ISlashCommand<FavoriteSongsSetSlashCommand.Options>
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
                    InteractionCustomId.Create(FavoriteSongsSetConfirmButtonHandler.CustomIdName)
                );
            }
            else
            {
                return new EmbedResult(await SetAsync(favoriteSongs, user, context));
            }
        }
    );

    public async ValueTask<Embed> SetAsync(string favoriteSongs, DiscordUser user, RunContext? context)
    {
        await favoriteSongsRepository.SetFavoriteSongsAsync(user, favoriteSongs);

        return EmbedFactory.CreateSuccess(
            $"""
            Your favorite songs list has been set successfully ✅
            Others can now use {mention.SlashCommand("favorite songs show", context)} to see your favorite songs 🎵
            """);
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Set(context.User, options.songs.Value, context));
    }
}

public class FavoriteSongsSetConfirmButtonHandler(
    InteractionResponseClient responseClient,
    IFavoriteSongsRepository favoriteSongsRepository,
    CommandMentioner mention) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.FavoriteSongsSetConfirm;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var promptMessage = button.Interaction.Raw.message;
        ArgumentNullException.ThrowIfNull(promptMessage);

        var songsEmbed = promptMessage.embeds.First(e => e.title?.Contains(FavoriteSongsShowSlashCommand.EmbedTitle, StringComparison.OrdinalIgnoreCase) == true);
        ArgumentNullException.ThrowIfNull(songsEmbed);
        ArgumentNullException.ThrowIfNull(songsEmbed.description);

        await favoriteSongsRepository.SetFavoriteSongsAsync(context.User, songsEmbed.description);

        await responseClient.EditOriginalResponseAsync(button.Interaction, InteractionMapper.ToInteractionEmbed(EmbedFactory.CreateSuccess(
            $"""
            Your favorite songs list has been set successfully ✅
            Others can now use {mention.SlashCommand("favorite songs show", context)} to see your favorite songs 🎵
            """)));
    }
}

public class FavoriteSongsClearSlashCommand(IFavoriteSongsRepository favoriteSongsRepository, CommandMentioner mention) : ISlashCommand<NoOptions>
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
                    You can set it again with {mention.SlashCommand("favorite songs set", context)}.
                    """));
            }
        ));
    }
}
