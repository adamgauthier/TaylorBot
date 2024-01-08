using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Commands;

public class ModMailLogSetSlashCommand(IModMailLogChannelRepository modMailLogChannelRepository) : ISlashCommand<ModMailLogSetSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("modmail log-set");

    public record Options(ParsedNonThreadTextChannelOrCurrent channel);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await modMailLogChannelRepository.AddOrUpdateModMailLogAsync(options.channel.Channel);

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(string.Join('\n', new[] {
                        $"Ok, I will now log mod mail in {options.channel.Channel.Mention}. ✅",
                        $"Use {context.MentionCommand("mod log stop")} to undo this action."
                    }))
                .Build());
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            }
        ));
    }
}

public class ModMailLogStopSlashCommand(IModMailLogChannelRepository modMailLogChannelRepository) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("modmail log-stop");

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await modMailLogChannelRepository.RemoveModMailLogAsync(context.Guild!);

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(string.Join('\n', new[] {
                        "Ok, I will stop logging mod mail in a different channel than your configured moderation log channel. ✅",
                        $"Use {context.MentionCommand("modmail log-set")} to change the mod mail log channel from the moderation log channel configured with {context.MentionCommand("mod log set")}."
                    }))
                .Build());
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            }
        ));
    }
}

public class ModMailLogShowSlashCommand(IModMailLogChannelRepository modMailLogChannelRepository) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("modmail log-show");

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild!;
                var modLog = await modMailLogChannelRepository.GetModMailLogForGuildAsync(guild);

                var embed = new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor);

                if (modLog != null)
                {
                    var channel = (ITextChannel?)await guild.GetChannelAsync(modLog.ChannelId.Id);
                    if (channel != null)
                    {
                        embed.WithDescription(string.Join('\n', new[] {
                            $"This server is configured to log mod mail in {channel.Mention}. ✅",
                            $"Use {context.MentionCommand("modmail log-stop")} to stop logging mod mail in a different channel than the one configured with {context.MentionCommand("mod log set")}."
                        }));
                    }
                    else
                    {
                        embed.WithDescription(string.Join('\n', new[] {
                            "I can't find the previously configured mod mail command usage logging channel in this server. ❌",
                            $"Was it deleted? Use {context.MentionCommand("modmail log-set")} to log mod mail in another channel."
                        }));
                    }
                }
                else
                {
                    embed.WithDescription(string.Join('\n', new[] {
                        "There is no mod mail specific logging channel configured in this server. ❌",
                        $"By default, mod mail logs will be sent in the moderation logging channel configured with {context.MentionCommand("mod log set")}.",
                        $"Use {context.MentionCommand("modmail log-set")} to log mod mail in a different channel."
                    }));
                }

                return new EmbedResult(embed.Build());
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            }
        ));
    }
}
