using Discord;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Client;
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

public class FavoriteObsessionShowSlashCommand(IObsessionRepository obsessionRepository, CommandMentioner mention) : ISlashCommand<FavoriteObsessionShowSlashCommand.Options>
{
    public const string PrefixCommandName = "waifu";

    public static string CommandName => "favorite obsession show";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserOrAuthor user);

    public const string EmbedTitle = "Obsession ❤️";

    public static Embed BuildDisplayEmbed(DiscordUser user, string favoriteObsession)
    {
        return new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithUserAsAuthor(user)
            .WithTitle(EmbedTitle)
            .WithImageUrl(favoriteObsession)
            .Build();
    }

    public Command Show(DiscordUser user, RunContext? context = null) => new(
        new(Info.Name, Aliases: [PrefixCommandName]),
        async () =>
        {
            var favoriteObsession = await obsessionRepository.GetObsessionAsync(user);

            if (favoriteObsession != null)
            {
                if (!Uri.TryCreate(favoriteObsession, UriKind.Absolute, out var url) ||
                    !url.IsWellFormedOriginalString() || url.Scheme is not ("http" or "https"))
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        The obsession for this user is not a valid URL to a photo! 😕
                        They need to use {mention.SlashCommand("favorite obsession set", context)} to update it.
                        """));
                }

                Embed embed = BuildDisplayEmbed(user, favoriteObsession);
                return new EmbedResult(embed);
            }
            else
            {
                return new EmbedResult(EmbedFactory.CreateError(
                    $"""
                    {user.Mention}'s obsession is not set. 🚫
                    They need to use {mention.SlashCommand("favorite obsession set", context)} to set it first.
                    """));
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Show(options.user.User, context));
    }
}

public class FavoriteObsessionSetSlashCommand : ISlashCommand<FavoriteObsessionSetSlashCommand.Options>
{
    public static string CommandName => "favorite obsession set";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedString obsession);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            () =>
            {
                var favoriteObsession = options.obsession.Value;
                if (!Uri.TryCreate(favoriteObsession, UriKind.Absolute, out var url) ||
                    !url.IsWellFormedOriginalString() || url.Scheme is not ("http" or "https"))
                {
                    return new(new EmbedResult(EmbedFactory.CreateError("The obsession you specified is not a valid URL to a photo 😕")));
                }

                var embed = FavoriteObsessionShowSlashCommand.BuildDisplayEmbed(context.User, favoriteObsession);

                return new(MessageResult.CreatePrompt(
                    new([embed, EmbedFactory.CreateWarning("Are you sure you want to set your obsession to the above?")]),
                    InteractionCustomId.Create(FavoriteObsessionSetConfirmButtonHandler.CustomIdName)
                ));
            }
        ));
    }
}

public class FavoriteObsessionSetConfirmButtonHandler(InteractionResponseClient responseClient, IObsessionRepository obsessionRepository, CommandMentioner mention) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.FavoriteObsessionSetConfirm;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var promptMessage = button.Interaction.Raw.message;
        ArgumentNullException.ThrowIfNull(promptMessage);

        var obsessionEmbed = promptMessage.embeds.First(e => e.title?.Contains(FavoriteObsessionShowSlashCommand.EmbedTitle, StringComparison.OrdinalIgnoreCase) == true);
        ArgumentNullException.ThrowIfNull(obsessionEmbed);

        var obsessionUrl = obsessionEmbed.image?.url;
        ArgumentNullException.ThrowIfNull(obsessionUrl);

        await obsessionRepository.SetObsessionAsync(context.User, obsessionUrl);

        await responseClient.EditOriginalResponseAsync(button.Interaction, InteractionMapper.ToInteractionEmbed(EmbedFactory.CreateSuccess(
            $"""
            Your obsession has been set successfully ✅
            Others can now use {mention.SlashCommand("favorite obsession show", context)} to see your obsession ❤️
            """)));
    }
}

public class FavoriteObsessionClearSlashCommand(IObsessionRepository obsessionRepository, CommandMentioner mention) : ISlashCommand<NoOptions>
{
    public static string CommandName => "favorite obsession clear";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

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
                    You can set it again with {mention.SlashCommand("favorite obsession set", context)}.
                    """));
            }
        ));
    }
}
