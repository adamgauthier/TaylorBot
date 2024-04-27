using Discord;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;

public interface IBaeRepository
{
    ValueTask<string?> GetBaeAsync(DiscordUser user);
    ValueTask SetBaeAsync(DiscordUser user, string bae);
    ValueTask ClearBaeAsync(DiscordUser user);
}

public class FavoriteBaeShowSlashCommand(IBaeRepository baeRepository) : ISlashCommand<FavoriteBaeShowSlashCommand.Options>
{
    public const string PrefixCommandName = "bae";

    public ISlashCommandInfo Info => new MessageCommandInfo("favorite bae show");

    public record Options(ParsedFetchedUserOrAuthor user);

    public static Embed BuildDisplayEmbed(DiscordUser user, string favoriteBae)
    {
        return new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithUserAsAuthor(user)
            .WithTitle("Bae ❤️")
            .WithDescription(favoriteBae)
            .Build();
    }

    public Command Show(DiscordUser user, RunContext? context = null) => new(
        new(Info.Name, Aliases: [PrefixCommandName]),
        async () =>
        {
            var favoriteBae = await baeRepository.GetBaeAsync(user);

            if (favoriteBae != null)
            {
                Embed embed = BuildDisplayEmbed(user, favoriteBae);
                return new EmbedResult(embed);
            }
            else
            {
                return new EmbedResult(EmbedFactory.CreateError(
                    $"""
                    {user.Mention}'s bae is not set. 🚫
                    They need to use {context?.MentionCommand("favorite bae set") ?? "</favorite bae set:1169468169140838502>"} to set it first.
                    """));
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Show(new(options.user.User), context));
    }
}

public class FavoriteBaeSetSlashCommand(IBaeRepository baeRepository) : ISlashCommand<FavoriteBaeSetSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("favorite bae set");

    public record Options(ParsedString bae);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        Command command = new(
            new(Info.Name),
            () =>
            {
                var embed = FavoriteBaeShowSlashCommand.BuildDisplayEmbed(context.User, options.bae.Value);

                return new(MessageResult.CreatePrompt(
                    new([embed, EmbedFactory.CreateWarning("Are you sure you want to set your bae to the above?")]),
                    confirm: async () => new(await SetAsync(options.bae.Value))
                ));

                async ValueTask<Embed> SetAsync(string favoriteBae)
                {
                    await baeRepository.SetBaeAsync(context.User, options.bae.Value);

                    return EmbedFactory.CreateSuccess(
                        $"""
                        Your bae has been set successfully. ✅
                        Others can now use {context?.MentionCommand("favorite bae show") ?? "</favorite bae show:1169468169140838502>"} to see your bae. ❤️
                        """);
                }
            }
        );
        return new(command);
    }
}

public class FavoriteBaeClearSlashCommand(IBaeRepository baeRepository) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("favorite bae clear");

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await baeRepository.ClearBaeAsync(context.User);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Your bae has been cleared and is no longer visible. ✅
                    You can set it again with {context.MentionCommand("favorite bae set")}.
                    """));
            }
        ));
    }
}
