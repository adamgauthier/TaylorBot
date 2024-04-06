using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;

public class ModLogSetSlashCommand(IModLogChannelRepository modLogChannelRepository) : ISlashCommand<ModLogSetSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("mod log set");

    public record Options(ParsedNonThreadTextChannelOrCurrent channel);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await modLogChannelRepository.AddOrUpdateModLogAsync(options.channel.Channel);

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        Ok, I will now log moderation command usage in {options.channel.Channel.Mention}. ✅
                        Use {context.MentionCommand("mod log stop")} to undo this action.
                        """)
                .Build());
            },
            Preconditions: [
                new InGuildPrecondition(botMustBeInGuild: true),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            ]
        ));
    }
}

public class ModLogStopSlashCommand(IModLogChannelRepository modLogChannelRepository) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("mod log stop");

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
                        Ok, I will stop logging moderation command usage in this server. ✅
                        Use {context.MentionCommand("mod log set")} to log moderation command usage in a specific channel.
                        """)
                .Build());
            },
            Preconditions: [
                new InGuildPrecondition(botMustBeInGuild: true),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            ]
        ));
    }
}

public class ModLogShowSlashCommand(IModLogChannelRepository modLogChannelRepository) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("mod log show");

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
                            This server is configured to log moderation command usage in {channel.Mention}. ✅
                            Use {context.MentionCommand("mod log stop")} to stop logging moderation command usage in this server.
                            """);
                    }
                    else
                    {
                        embed.WithDescription(
                            $"""
                            I can't find the previously configured moderation command usage logging channel in this server. ❌
                            Was it deleted? Use {context.MentionCommand("mod log set")} to log moderation command usage in another channel.
                            """);
                    }
                }
                else
                {
                    embed.WithDescription(
                        $"""
                        There is no moderation command usage logging configured in this server. ❌",
                        "Use {context.MentionCommand("mod log set")} to log moderation command usage in a specific channel.
                        """);
                }

                return new EmbedResult(embed.Build());
            },
            Preconditions: [
                new InGuildPrecondition(botMustBeInGuild: true),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            ]
        ));
    }
}
