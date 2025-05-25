using Discord;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Client;
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

public class FavoriteBaeShowSlashCommand(IBaeRepository baeRepository, CommandMentioner mention) : ISlashCommand<FavoriteBaeShowSlashCommand.Options>
{
    public const string PrefixCommandName = "bae";

    public static string CommandName => "favorite bae show";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserOrAuthor user);

    public const string EmbedTitle = "Bae ❤️";

    public static Embed BuildDisplayEmbed(DiscordUser user, string favoriteBae)
    {
        return new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithUserAsAuthor(user)
            .WithTitle(EmbedTitle)
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
                    They need to use {mention.SlashCommand("favorite bae set", context)} to set it first.
                    """));
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Show(options.user.User, context));
    }
}

public class FavoriteBaeSetSlashCommand : ISlashCommand<FavoriteBaeSetSlashCommand.Options>
{
    public static string CommandName => "favorite bae set";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

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
                    InteractionCustomId.Create(FavoriteBaeSetConfirmButtonHandler.CustomIdName)
                ));
            }
        );
        return new(command);
    }
}

public class FavoriteBaeSetConfirmButtonHandler(IInteractionResponseClient responseClient, IBaeRepository baeRepository, CommandMentioner mention) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.FavoriteBaeSetConfirm;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var promptMessage = button.Interaction.Raw.message;
        ArgumentNullException.ThrowIfNull(promptMessage);

        var baeEmbed = promptMessage.embeds.First(e => e.title?.Contains(FavoriteBaeShowSlashCommand.EmbedTitle, StringComparison.OrdinalIgnoreCase) == true);
        ArgumentNullException.ThrowIfNull(baeEmbed);
        ArgumentNullException.ThrowIfNull(baeEmbed.description);

        await baeRepository.SetBaeAsync(context.User, baeEmbed.description);

        await responseClient.EditOriginalResponseAsync(button.Interaction, InteractionMapper.ToInteractionEmbed(EmbedFactory.CreateSuccess(
            $"""
            Your bae has been set successfully ✅
            Others can now use {mention.SlashCommand("favorite bae show", context)} to see your bae ❤️
            """)));
    }
}

public class FavoriteBaeClearSlashCommand(IBaeRepository baeRepository, CommandMentioner mention) : ISlashCommand<NoOptions>
{
    public static string CommandName => "favorite bae clear";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

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
                    You can set it again with {mention.SlashCommand("favorite bae set", context)}.
                    """));
            }
        ));
    }
}
