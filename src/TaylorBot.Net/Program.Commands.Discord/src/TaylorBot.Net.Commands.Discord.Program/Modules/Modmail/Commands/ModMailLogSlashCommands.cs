using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Commands;

public class ModMailLogSetSlashCommand(
    IModMailLogChannelRepository modMailLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission) : ISlashCommand<ModMailLogSetSlashCommand.Options>
{
    public static string CommandName => "modmail log-set";

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

                var modMailLog = await modMailLogChannelRepository.GetModMailLogForGuildAsync(guild);
                if (modMailLog == null)
                {
                    return new EmbedResult(await AddOrUpdateAsync(context, options));
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
        await modMailLogChannelRepository.AddOrUpdateModMailLogAsync(options.channel.Channel);

        return new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithDescription(
                $"""
                Ok, I will now log mod mail in {options.channel.Channel.Mention} ✅
                Use {context.MentionSlashCommand("modmail log-stop")} to undo this action ↩️
                """)
        .Build();
    }
}

public class ModMailLogStopSlashCommand(
    IModMailLogChannelRepository modMailLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission) : ISlashCommand<NoOptions>
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
                        Use {context.MentionSlashCommand("modmail log-set")} to change the mod mail log channel from the moderation log channel configured with {context.MentionSlashCommand("mod log set")}
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

public class ModMailLogShowSlashCommand(
    IModMailLogChannelRepository modMailLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission) : ISlashCommand<NoOptions>
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
                            Use {context.MentionSlashCommand("modmail log-stop")} to stop logging mod mail in a different channel than the one configured with {context.MentionSlashCommand("mod log set")}
                            """);
                    }
                    else
                    {
                        embed.WithDescription(
                            $"""
                            I can't find the previously configured mod mail command usage logging channel in this server ❌
                            Was it deleted? Use {context.MentionSlashCommand("modmail log-set")} to log mod mail in another channel
                            """);
                    }
                }
                else
                {
                    embed.WithDescription(
                        $"""
                        There is no mod mail specific logging channel configured in this server ❌
                        By default, mod mail logs will be sent in the moderation logging channel configured with {context.MentionSlashCommand("mod log set")}
                        Use {context.MentionSlashCommand("modmail log-set")} to log mod mail in a different channel
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
