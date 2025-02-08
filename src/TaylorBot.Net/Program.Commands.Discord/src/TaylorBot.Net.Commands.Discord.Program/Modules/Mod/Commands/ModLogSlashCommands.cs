using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Commands;

public class ModLogSetSlashCommand(
    IModLogChannelRepository modLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission) : ISlashCommand<ModLogSetSlashCommand.Options>
{
    public static string CommandName => "mod log set";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedNonThreadTextChannelOrCurrent channel);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var modLog = await modLogChannelRepository.GetModLogForGuildAsync(guild);
                if (modLog == null)
                {
                    return new EmbedResult(await AddOrUpdateAsync(context, options));
                }
                else
                {
                    return MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(
                            $"""
                            Are you sure you want to change the mod log channel to {options.channel.Channel.Mention}? ⚠️
                            Moderation command usage is currently being logged to {MentionUtils.MentionChannel(modLog.ChannelId)} 👈
                            """
                        )),
                        confirm: async () => new(await AddOrUpdateAsync(context, options))
                    );
                }
            },
            Preconditions: [
                new InGuildPrecondition(botMustBeInGuild: true),
                userHasPermission.Create(GuildPermission.ManageGuild)
            ]
        ));
    }

    private async ValueTask<Embed> AddOrUpdateAsync(RunContext context, Options options)
    {
        await modLogChannelRepository.AddOrUpdateModLogAsync(options.channel.Channel);

        return new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithDescription(
                $"""
                Ok, I will now log moderation command usage in {options.channel.Channel.Mention} ✅
                Use {context.MentionCommand("mod log stop")} to undo this action ↩️
                """)
        .Build();
    }
}

public class ModLogStopSlashCommand(
    IModLogChannelRepository modLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission) : ISlashCommand<NoOptions>
{
    public static string CommandName => "mod log stop";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                await modLogChannelRepository.RemoveModLogAsync(guild);

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        Ok, I will stop logging moderation command usage in this server ✅
                        Use {context.MentionCommand("mod log set")} to log moderation command usage in a specific channel ↩️
                        """)
                .Build());
            },
            Preconditions: [
                new InGuildPrecondition(botMustBeInGuild: true),
                userHasPermission.Create(GuildPermission.ManageGuild)
            ]
        ));
    }
}

public class ModLogShowSlashCommand(
    IModLogChannelRepository modLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission) : ISlashCommand<NoOptions>
{
    public static string CommandName => "mod log show";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var modLog = await modLogChannelRepository.GetModLogForGuildAsync(guild);

                var embed = new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor);

                if (modLog != null)
                {
                    var channel = (ITextChannel?)await guild.GetChannelAsync(modLog.ChannelId.Id);
                    if (channel != null)
                    {
                        embed.WithDescription(
                            $"""
                            This server is configured to log moderation command usage in {channel.Mention} ✅
                            Use {context.MentionCommand("mod log stop")} to stop logging moderation command usage in this server ↩️
                            """);
                    }
                    else
                    {
                        embed.WithDescription(
                            $"""
                            I can't find the previously configured moderation command usage logging channel in this server ❌
                            Was it deleted? Use {context.MentionCommand("mod log set")} to log moderation command usage in another channel ↩️
                            """);
                    }
                }
                else
                {
                    embed.WithDescription(
                        $"""
                        There is no moderation command usage logging configured in this server ❌
                        "Use {context.MentionCommand("mod log set")} to log moderation command usage in a specific channel ↩️
                        """);
                }

                return new EmbedResult(embed.Build());
            },
            Preconditions: [
                new InGuildPrecondition(botMustBeInGuild: true),
                userHasPermission.Create(GuildPermission.ManageGuild)
            ]
        ));
    }
}
