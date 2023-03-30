using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Commands;

public class ModMailBlockSlashCommand : ISlashCommand<ModMailBlockSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("modmail block", IsPrivateResponse: true);

    public record Options(ParsedUserNotAuthorAndBot user);

    private static readonly Color EmbedColor = new(255, 100, 100);

    private readonly IModMailBlockedUsersRepository _modMailBlockedUsersRepository;
    private readonly ModMailChannelLogger _modMailChannelLogger;
    private readonly IPlusRepository _plusRepository;

    public ModMailBlockSlashCommand(IModMailBlockedUsersRepository modMailBlockedUsersRepository, ModMailChannelLogger modMailChannelLogger, IPlusRepository plusRepository)
    {
        _modMailBlockedUsersRepository = modMailBlockedUsersRepository;
        _modMailChannelLogger = modMailChannelLogger;
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

                var wasLogged = await _modMailChannelLogger.TrySendModMailLogAsync(guild, context.User, user, logEmbed =>
                    logEmbed
                        .WithColor(EmbedColor)
                        .WithFooter("User blocked from sending mod mail")
                );

                return new EmbedResult(_modMailChannelLogger.CreateResultEmbed(context, wasLogged, string.Join('\n', new[] {
                    $"Blocked {user.FormatTagAndMention()} from sending mod mail in this server. 👍",
                    $"You can undo this action with {context.MentionCommand("mod mail unblock")}."
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
    public ISlashCommandInfo Info => new MessageCommandInfo("modmail unblock", IsPrivateResponse: true);

    public record Options(ParsedUserNotAuthorAndBot user);

    private static readonly Color EmbedColor = new(205, 120, 230);

    private readonly IModMailBlockedUsersRepository _modMailBlockedUsersRepository;
    private readonly ModMailChannelLogger _modMailChannelLogger;

    public ModMailUnblockSlashCommand(IModMailBlockedUsersRepository modMailBlockedUsersRepository, ModMailChannelLogger modMailChannelLogger)
    {
        _modMailBlockedUsersRepository = modMailBlockedUsersRepository;
        _modMailChannelLogger = modMailChannelLogger;
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

                var wasLogged = await _modMailChannelLogger.TrySendModMailLogAsync(guild, context.User, user, logEmbed =>
                    logEmbed
                        .WithColor(EmbedColor)
                        .WithFooter("User unblocked from sending mod mail")
                );

                return new EmbedResult(_modMailChannelLogger.CreateResultEmbed(context, wasLogged, string.Join('\n', new[] {
                    $"Unblocked {user.FormatTagAndMention()} from sending mod mail in this server. 👍",
                    $"You can block again with {context.MentionCommand("mod mail block")}."
                })));
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.BanMembers)
            }
        ));
    }
}
