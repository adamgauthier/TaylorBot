using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands
{
    public class ModMailBlockSlashCommand : ISlashCommand<ModMailBlockSlashCommand.Options>
    {
        public SlashCommandInfo Info => new("mod mail block", IsPrivateResponse: true);

        public record Options(ParsedUserNotAuthorAndBot user);

        private static readonly Color EmbedColor = new(255, 100, 100);

        private readonly IModMailBlockedUsersRepository _modMailBlockedUsersRepository;
        private readonly IModChannelLogger _modChannelLogger;
        private readonly IPlusRepository _plusRepository;

        public ModMailBlockSlashCommand(IModMailBlockedUsersRepository modMailBlockedUsersRepository, IModChannelLogger modChannelLogger, IPlusRepository plusRepository)
        {
            _modMailBlockedUsersRepository = modMailBlockedUsersRepository;
            _modChannelLogger = modChannelLogger;
            _plusRepository = plusRepository;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new Command(
                new(Info.Name),
                async () =>
                {
                    var guild = context.Guild!;
                    var user = options.user.User;

                    var blockedUserCount = await _modMailBlockedUsersRepository.GetBlockedUserCountAsync(guild);

                    var isPlus = await _plusRepository.IsActivePlusGuildAsync(guild);

                    if (!isPlus)
                    {
                        const int MaxBlockedUsersPerGuild = 50;

                        if (blockedUserCount >= MaxBlockedUsersPerGuild)
                        {
                            return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                                $"You've reached the limit of blocked users ({MaxBlockedUsersPerGuild}). 😕",
                                $"Consider making this server a TaylorBot Plus server (`{context.CommandPrefix}plus add`) to remove this limit."
                            })));
                        }
                    }

                    await _modMailBlockedUsersRepository.BlockAsync(guild, user);

                    var wasLogged = await _modChannelLogger.TrySendModLogAsync(guild, context.User, user, logEmbed =>
                        logEmbed
                            .WithColor(EmbedColor)
                            .WithFooter("User blocked from sending mod mail")
                    );

                    return new EmbedResult(_modChannelLogger.CreateResultEmbed(wasLogged, string.Join('\n', new[] {
                        $"Blocked {user.FormatTagAndMention()} from sending mod mail in this server. 👍",
                        "You can undo this action with `/mod mail unblock`."
                    })));
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new UserHasPermissionOrOwnerPrecondition(GuildPermission.BanMembers)
                }
            ));
        }
    }

    public class ModMailUnblockSlashCommand : ISlashCommand<ModMailUnblockSlashCommand.Options>
    {
        public SlashCommandInfo Info => new("mod mail unblock", IsPrivateResponse: true);

        public record Options(ParsedUserNotAuthorAndBot user);

        private static readonly Color EmbedColor = new(205, 120, 230);

        private readonly IModMailBlockedUsersRepository _modMailBlockedUsersRepository;
        private readonly IModChannelLogger _modChannelLogger;

        public ModMailUnblockSlashCommand(IModMailBlockedUsersRepository modMailBlockedUsersRepository, IModChannelLogger modChannelLogger)
        {
            _modMailBlockedUsersRepository = modMailBlockedUsersRepository;
            _modChannelLogger = modChannelLogger;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new Command(
                new(Info.Name),
                async () =>
                {
                    var guild = context.Guild!;
                    var user = options.user.User;

                    await _modMailBlockedUsersRepository.UnblockAsync(guild, user);

                    var wasLogged = await _modChannelLogger.TrySendModLogAsync(guild, context.User, user, logEmbed =>
                        logEmbed
                            .WithColor(EmbedColor)
                            .WithFooter("User unblocked from sending mod mail")
                    );

                    return new EmbedResult(_modChannelLogger.CreateResultEmbed(wasLogged, string.Join('\n', new[] {
                        $"Unblocked {user.FormatTagAndMention()} from sending mod mail in this server. 👍",
                        "You can block again with `/mod mail block`."
                    })));
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new UserHasPermissionOrOwnerPrecondition(GuildPermission.BanMembers)
                }
            ));
        }
    }
}
