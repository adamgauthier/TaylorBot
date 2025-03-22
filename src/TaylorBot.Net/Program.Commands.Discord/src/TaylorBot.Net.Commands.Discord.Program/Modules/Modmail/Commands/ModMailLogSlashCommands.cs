using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Commands;

public class ModMailLogSetSlashCommand(
    IModMailLogChannelRepository modMailLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    CommandMentioner mention,
    InGuildPrecondition.Factory inGuild) : ISlashCommand<ModMailLogSetSlashCommand.Options>
{
    public static string CommandName => "modmail log-set";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public IList<ICommandPrecondition> BuildPreconditions() => [
        inGuild.Create(botMustBeInGuild: true),
        userHasPermission.Create(GuildPermission.ManageGuild)
    ];

    public record Options(ParsedNonThreadTextChannelOrCurrent channel);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var modMailLog = await modMailLogChannelRepository.GetModMailLogForGuildAsync(guild);
                if (modMailLog == null)
                {
                    return new EmbedResult(await AddOrUpdateAsync(context, options.channel.Channel));
                }
                else
                {
                    return MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(
                            $"""
                            Are you sure you want to change the mod mail log channel to {options.channel.Channel.Mention}? ⚠️
                            Mod mail is currently being logged to {MentionUtils.MentionChannel(modMailLog.ChannelId)} 👈
                            """
                        )),
                        InteractionCustomId.Create(ModMailLogSetConfirmButtonHandler.CustomIdName, [new("channel", $"{options.channel.Channel.Id}")])
                    );
                }
            },
            Preconditions: BuildPreconditions()
        ));
    }

    public async ValueTask<Embed> AddOrUpdateAsync(RunContext context, GuildTextChannel channel)
    {
        await modMailLogChannelRepository.AddOrUpdateModMailLogAsync(channel);

        return new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithDescription(
                $"""
                Ok, I will now log mod mail in {channel.Mention} ✅
                Use {mention.SlashCommand("modmail log-stop", context)} to undo this action ↩️
                """)
        .Build();
    }
}

public class ModMailLogSetConfirmButtonHandler(InteractionResponseClient responseClient, ModMailLogSetSlashCommand command) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.ModMailLogSetConfirm;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        Preconditions: command.BuildPreconditions(),
        RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var guild = context.Guild?.Fetched;
        ArgumentNullException.ThrowIfNull(guild);

        SnowflakeId channelId = button.CustomId.ParsedData["channel"];
        var channel = await guild.GetChannelAsync(channelId);

        if (channel == null)
        {
            await responseClient.EditOriginalResponseAsync(button.Interaction, EmbedFactory.CreateErrorEmbed(
                $"The selected channel {MentionUtils.MentionChannel(channelId)} no longer exists 🤔"));
            return;
        }

        GuildTextChannel textChannel = new(channelId, guild.Id, channel.ChannelType);
        var embed = await command.AddOrUpdateAsync(context, textChannel);

        await responseClient.EditOriginalResponseAsync(button.Interaction, InteractionMapper.ToInteractionEmbed(embed));
    }
}

public class ModMailLogStopSlashCommand(
    IModMailLogChannelRepository modMailLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    CommandMentioner mention,
    InGuildPrecondition.Factory inGuild) : ISlashCommand<NoOptions>
{
    public static string CommandName => "modmail log-stop";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                await modMailLogChannelRepository.RemoveModMailLogAsync(guild);

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        Ok, I will stop logging mod mail in a different channel than your configured moderation log channel ✅
                        Use {mention.SlashCommand("modmail log-set", context)} to change the mod mail log channel from the moderation log channel configured with {mention.SlashCommand("mod log set", context)}
                        """)
                .Build());
            },
            Preconditions: [
                inGuild.Create(botMustBeInGuild: true),
                userHasPermission.Create(GuildPermission.ManageGuild)
            ]
        ));
    }
}

public class ModMailLogShowSlashCommand(
    IModMailLogChannelRepository modMailLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    CommandMentioner mention,
    InGuildPrecondition.Factory inGuild) : ISlashCommand<NoOptions>
{
    public static string CommandName => "modmail log-show";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var modMailLog = await modMailLogChannelRepository.GetModMailLogForGuildAsync(guild);

                var embed = new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor);

                if (modMailLog != null)
                {
                    var channel = (ITextChannel?)await guild.GetChannelAsync(modMailLog.ChannelId.Id);
                    if (channel != null)
                    {
                        embed.WithDescription(
                            $"""
                            This server is configured to log mod mail in {channel.Mention} ✅
                            Use {mention.SlashCommand("modmail log-stop", context)} to stop logging mod mail in a different channel than the one configured with {mention.SlashCommand("mod log set", context)}
                            """);
                    }
                    else
                    {
                        embed.WithDescription(
                            $"""
                            I can't find the previously configured mod mail command usage logging channel in this server ❌
                            Was it deleted? Use {mention.SlashCommand("modmail log-set", context)} to log mod mail in another channel
                            """);
                    }
                }
                else
                {
                    embed.WithDescription(
                        $"""
                        There is no mod mail specific logging channel configured in this server ❌
                        By default, mod mail logs will be sent in the moderation logging channel configured with {mention.SlashCommand("mod log set", context)}
                        Use {mention.SlashCommand("modmail log-set", context)} to log mod mail in a different channel
                        """);
                }

                return new EmbedResult(embed.Build());
            },
            Preconditions: [
                inGuild.Create(botMustBeInGuild: true),
                userHasPermission.Create(GuildPermission.ManageGuild)
            ]
        ));
    }
}
