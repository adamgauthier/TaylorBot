using Discord;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;

public interface IFavoriteSongsRepository
{
    ValueTask<string?> GetFavoriteSongsAsync(IUser user);
    ValueTask SetFavoriteSongsAsync(IUser user, string songs);
    ValueTask ClearFavoriteSongsAsync(IUser user);
}

public class FavoriteSongsShowSlashCommand : ISlashCommand<FavoriteSongsShowSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("favorite songs show");

    public record Options(ParsedUserOrAuthor user);

    private readonly IFavoriteSongsRepository _favoriteSongsRepository;

    public FavoriteSongsShowSlashCommand(IFavoriteSongsRepository favoriteSongsRepository)
    {
        _favoriteSongsRepository = favoriteSongsRepository;
    }

    public static Embed BuildDisplayEmbed(IUser user, string favoriteSongs)
    {
        return new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithUserAsAuthor(user)
            .WithTitle("Favorite Songs List 🎵")
            .WithDescription(favoriteSongs)
        .Build();
    }

    public Command Show(IUser user, RunContext? context = null) => new(
        new(Info.Name),
        async () =>
        {
            var favoriteSongs = await _favoriteSongsRepository.GetFavoriteSongsAsync(user);

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
                    They need to use {context?.MentionCommand("favorite songs set") ?? "</favorite songs set:1169468169140838502>"} to set it first.
                    """));
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Show(options.user.User, context));
    }
}

public class FavoriteSongsSetSlashCommand : ISlashCommand<FavoriteSongsSetSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("favorite songs set");

    public record Options(ParsedString songs);

    private readonly IFavoriteSongsRepository _favoriteSongsRepository;

    public FavoriteSongsSetSlashCommand(IFavoriteSongsRepository favoriteSongsRepository)
    {
        _favoriteSongsRepository = favoriteSongsRepository;
    }

    public Command Set(IUser user, string favoriteSongs, RunContext? context = null) => new(
        new(Info.Name),
        async () =>
        {
            if (context != null)
            {
                favoriteSongs = string.Join(
                    '\n',
                    favoriteSongs.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));

                var embed = FavoriteSongsShowSlashCommand.BuildDisplayEmbed(user, favoriteSongs);

                return MessageResult.CreatePrompt(
                    new(new[] { embed, EmbedFactory.CreateWarning("Are you sure you want to set your favorite songs list to the above?") }),
                    confirm: async () => new(await SetAsync(favoriteSongs))
                );
            }
            else
            {
                return new EmbedResult(await SetAsync(favoriteSongs));
            }

            async ValueTask<Embed> SetAsync(string favoriteSongs)
            {
                await _favoriteSongsRepository.SetFavoriteSongsAsync(user, favoriteSongs);

                return EmbedFactory.CreateSuccess(
                    $"""
                    Your favorite songs list has been set successfully. ✅
                    Others can now use {context?.MentionCommand("favorite songs show") ?? "</favorite songs show:1169468169140838502>"} to see your favorite songs. 🎵
                    """);
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Set(context.User, options.songs.Value, context));
    }
}

public class FavoriteSongsClearSlashCommand : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("favorite songs clear");

    private readonly IFavoriteSongsRepository _favoriteSongsRepository;

    public FavoriteSongsClearSlashCommand(IFavoriteSongsRepository favoriteSongsRepository)
    {
        _favoriteSongsRepository = favoriteSongsRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await _favoriteSongsRepository.ClearFavoriteSongsAsync(context.User);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Your favorite songs list has been cleared and is no longer be visible. ✅
                    You can set it again with {context.MentionCommand("favorite songs set")}.
                    """));
            }
        ));
    }
}
