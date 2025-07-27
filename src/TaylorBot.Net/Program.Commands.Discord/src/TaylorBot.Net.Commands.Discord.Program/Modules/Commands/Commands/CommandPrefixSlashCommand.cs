using Discord;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Commands.Commands;

public class CommandPrefixSlashCommand(
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    CommandMentioner mention,
    InGuildPrecondition.Factory inGuild,
    ICommandPrefixRepository commandPrefixRepository,
    IDisabledGuildCommandRepository disabledGuildCommandRepository) : ISlashCommand<NoOptions>
{
    public static string CommandName => "command prefix";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public IList<ICommandPrecondition> BuildPreconditions() => [
        inGuild.Create(botMustBeInGuild: true),
        userHasPermission.Create(GuildPermission.ManageGuild)
    ];

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions _)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild;
                ArgumentNullException.ThrowIfNull(guild);
                ArgumentNullException.ThrowIfNull(guild.Fetched);

                var prefixResult = await commandPrefixRepository.GetOrInsertGuildPrefixAsync(guild.Fetched);
                var currentPrefix = prefixResult.Prefix;

                var prefixDisabledResult = await disabledGuildCommandRepository.IsGuildCommandDisabledAsync(guild, new("all-prefix"));
                var arePrefixCommandsDisabled = prefixDisabledResult.IsDisabled;

                var description =
                    $"""
                    ## Prefix Commands 👴
                    Prefix commands are commands that are used in text channels by typing a prefix followed by the command name 💬
                    The default prefix for TaylorBot is `!`, allowing you to use commands by typing `!help` for example ❗
                    ## This Server's Prefix 🖌️
                    - **Current server prefix:** `{currentPrefix}`
                    One issue with prefix commands is that multiple bots can use the same prefix 🪢
                    This means typing `!help` could trigger multiple bots at once, which can be confusing/spammy 😵
                    To solve this, TaylorBot allows you to change the prefix for your server with "<@119572982178906114> setprefix" command ⚙️
                    ## Disabling Prefix Commands 🚫
                    
                    """;

                InteractionComponent button;
                if (arePrefixCommandsDisabled)
                {
                    description +=
                        $"""
                        Prefix commands are currently **disabled** in this server ⛔
                        Users must use slash commands (e.g. {mention.SlashCommand("help")}) instead of prefix commands like `{currentPrefix}help` ✅
                        You can re-enable prefix commands using the button below ⬇️
                        """;
                    button = CreateEnablePrefixButton();
                }
                else
                {
                    description +=
                        $"""
                        While prefix commands used to be the only way to interact with bots on Discord, they have many downsides 🥲
                        Slash commands are now the preferred modern way to interact with bots, with many improvements such as buttons, modals, etc. 💪
                        For example, you can use {mention.SlashCommand("help")} instead of typing `{currentPrefix}help` ⚡
                        
                        Some servers may want to disable prefix commands entirely to avoid confusion and encourage users to use slash commands instead 🐣
                        Use the button below to disable prefix commands in this server ⬇️
                        """;
                    button = CreateDisablePrefixButton();
                }

                var embed = EmbedFactory.CreateSuccess(description);
                return new MessageResult(new(new(embed), [button]));
            },
            Preconditions: BuildPreconditions()
        ));
    }

    public static InteractionComponent CreateDisablePrefixButton()
    {
        return InteractionComponent.CreateActionRow(InteractionComponent.CreateButton(
            style: InteractionButtonStyle.Danger,
            custom_id: InteractionCustomId.Create(CustomIdNames.CommandPrefixToggle, [new("enable", bool.FalseString)]).RawId,
            label: "Disable prefix commands"));
    }

    public static InteractionComponent CreateEnablePrefixButton()
    {
        return InteractionComponent.CreateActionRow(InteractionComponent.CreateButton(
            style: InteractionButtonStyle.Success,
            custom_id: InteractionCustomId.Create(CustomIdNames.CommandPrefixToggle, [new("enable", bool.TrueString)]).RawId,
            label: "Enable prefix commands"));
    }
}

public class CommandPrefixToggleHandler(
    IInteractionResponseClient responseClient,
    CommandPrefixSlashCommand command,
    IDisabledGuildCommandRepository disabledGuildCommandRepository,
    CommandMentioner mention,
    ICommandPrefixRepository commandPrefixRepository
) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.CommandPrefixToggle;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        Preconditions: command.BuildPreconditions(),
        RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var guild = context.Guild;
        ArgumentNullException.ThrowIfNull(guild);
        ArgumentNullException.ThrowIfNull(guild.Fetched);

        var enable = button.CustomId.ParsedData.TryGetValue("enable", out var enableStr)
            && bool.TryParse(enableStr, out var parsed) && parsed == true;

        if (enable)
        {
            await disabledGuildCommandRepository.EnableInAsync(guild.Fetched, "all-prefix");

            var prefixResult = await commandPrefixRepository.GetOrInsertGuildPrefixAsync(guild.Fetched);
            var currentPrefix = prefixResult.Prefix;

            var embed = EmbedFactory.CreateSuccessEmbed(
                $"""
                Prefix commands are now enabled in this server ✅
                Users can use prefix commands like `{currentPrefix}help` again 👴
                You can disable prefix commands at any time using {mention.SlashCommand("command prefix")} ↩️
                """);
            await responseClient.EditOriginalResponseAsync(button.Interaction, embed);
        }
        else
        {
            await disabledGuildCommandRepository.DisableInAsync(guild.Fetched, "all-prefix");

            var embed = EmbedFactory.CreateSuccessEmbed(
                $"""
                Prefix commands are now disabled in this server ✅
                Users will need to use slash commands instead 💪
                You can re-enable prefix commands at any time using {mention.SlashCommand("command prefix")} ↩️
                """);
            await responseClient.EditOriginalResponseAsync(button.Interaction, embed);
        }
    }
}
