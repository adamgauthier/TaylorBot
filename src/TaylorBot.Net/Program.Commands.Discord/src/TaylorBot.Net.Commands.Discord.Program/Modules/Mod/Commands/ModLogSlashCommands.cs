using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.Events;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands
{
    public class ModLogSetSlashCommand : ISlashCommand<ModLogSetSlashCommand.Options>
    {
        public string Name => "mod log set";

        public record Options(ParsedTextChannelOrCurrent channel);

        private readonly IModLogChannelRepository _modLogChannelRepository;

        public ModLogSetSlashCommand(IModLogChannelRepository modLogChannelRepository)
        {
            _modLogChannelRepository = modLogChannelRepository;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new Command(
                new(Name),
                async () =>
                {
                    await _modLogChannelRepository.AddOrUpdateModLogAsync(options.channel.Channel);

                    return new EmbedResult(new EmbedBuilder()
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription(string.Join('\n', new[] {
                            $"Ok, I will now log moderation command usage in {options.channel.Channel.Mention}. ✅",
                            $"Use `/mod log stop` to undo this action."
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

    public class ModLogStopSlashCommand : ISlashCommand<NoOptions>
    {
        public string Name => "mod log stop";

        private readonly IModLogChannelRepository _modLogChannelRepository;

        public ModLogStopSlashCommand(IModLogChannelRepository modLogChannelRepository)
        {
            _modLogChannelRepository = modLogChannelRepository;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
        {
            return new(new Command(
                new(Name),
                async () =>
                {
                    await _modLogChannelRepository.RemoveModLogAsync(context.Guild!);

                    return new EmbedResult(new EmbedBuilder()
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription(string.Join('\n', new[] {
                            "Ok, I will stop logging moderation command usage in this server. ✅",
                            "Use `/mod log set` to log moderation command usage in a specific channel."
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

    public class ModLogShowSlashCommand : ISlashCommand<NoOptions>
    {
        public string Name => "mod log show";

        private readonly IModLogChannelRepository _modLogChannelRepository;

        public ModLogShowSlashCommand(IModLogChannelRepository modLogChannelRepository)
        {
            _modLogChannelRepository = modLogChannelRepository;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
        {
            return new(new Command(
                new(Name),
                async () =>
                {
                    var guild = context.Guild!;
                    var modLog = await _modLogChannelRepository.GetModLogForGuildAsync(guild);

                    var embed = new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor);

                    if (modLog != null)
                    {
                        var channel = (ITextChannel?)await guild.GetChannelAsync(modLog.ChannelId.Id);
                        if (channel != null)
                        {
                            embed.WithDescription(string.Join('\n', new[] {
                                $"This server is configured to log moderation command usage in {channel.Mention}. ✅",
                                "Use `/mod log stop` to stop logging moderation command usage in this server."
                            }));
                        }
                        else
                        {
                            embed.WithDescription(string.Join('\n', new[] {
                                "I can't find the previously configured moderation command usage logging channel in this server. ❌",
                                "Was it deleted? Use `/mod log set` to log moderation command usage in another channel."
                            }));
                        }
                    }
                    else
                    {
                        embed.WithDescription(string.Join('\n', new[] {
                            "There is no moderation command usage logging configured in this server. ❌",
                            "Use `/mod log set` to log moderation command usage in a specific channel."
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
}
