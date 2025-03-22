using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Commands;

public class ModMailBlockSlashCommand(
    IModMailBlockedUsersRepository modMailBlockedUsersRepository,
    ModMailChannelLogger modMailChannelLogger,
    IPlusRepository plusRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    InGuildPrecondition.Factory inGuild,
    CommandMentioner mention) : ISlashCommand<ModMailBlockSlashCommand.Options>
{
    public static string CommandName => "modmail block";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName, IsPrivateResponse: true);

    public record Options(ParsedUserNotAuthorAndBot user);

    private static readonly Color EmbedColor = new(255, 100, 100);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild;
                ArgumentNullException.ThrowIfNull(guild);
                ArgumentNullException.ThrowIfNull(guild.Fetched);

                var user = options.user.User;

                var blockedUserCount = await modMailBlockedUsersRepository.GetBlockedUserCountAsync(guild.Fetched);

                var isPlus = await plusRepository.IsActivePlusGuildAsync(guild);

                if (!isPlus)
                {
                    const int MaxBlockedUsersPerGuild = 50;

                    if (blockedUserCount >= MaxBlockedUsersPerGuild)
                    {
                        return new EmbedResult(EmbedFactory.CreateError(
                            $"""
                            You've reached the limit of blocked users ({MaxBlockedUsersPerGuild}) 😕
                            Use {mention.SlashCommand("plus add", context)} to remove this limit 💎
                            """));
                    }
                }

                await modMailBlockedUsersRepository.BlockAsync(guild.Fetched, user);

                var wasLogged = await modMailChannelLogger.TrySendModMailLogAsync(guild.Fetched, context.User, user, logEmbed =>
                    logEmbed
                        .WithColor(EmbedColor)
                        .WithFooter("User blocked from sending mod mail")
                );

                return new EmbedResult(modMailChannelLogger.CreateResultEmbed(context, wasLogged,
                    $"""
                    Blocked {user.FormatTagAndMention()} from sending mod mail in this server. 👍
                    You can undo this action with {mention.SlashCommand("mod mail unblock", context)}.
                    """));
            },
            Preconditions: [
                inGuild.Create(botMustBeInGuild: true),
                userHasPermission.Create(GuildPermission.BanMembers)
            ]
        ));
    }
}

public class ModMailUnblockSlashCommand(
    IModMailBlockedUsersRepository modMailBlockedUsersRepository,
    ModMailChannelLogger modMailChannelLogger,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    InGuildPrecondition.Factory inGuild,
    CommandMentioner mention) : ISlashCommand<ModMailUnblockSlashCommand.Options>
{
    public static string CommandName => "modmail unblock";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName, IsPrivateResponse: true);

    public record Options(ParsedUserNotAuthorAndBot user);

    private static readonly Color EmbedColor = new(205, 120, 230);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var user = options.user.User;

                await modMailBlockedUsersRepository.UnblockAsync(guild, user);

                var wasLogged = await modMailChannelLogger.TrySendModMailLogAsync(guild, context.User, user, logEmbed =>
                    logEmbed
                        .WithColor(EmbedColor)
                        .WithFooter("User unblocked from sending mod mail")
                );

                return new EmbedResult(modMailChannelLogger.CreateResultEmbed(context, wasLogged,
                    $"""
                    Unblocked {user.FormatTagAndMention()} from sending mod mail in this server. 👍
                    You can block again with {mention.SlashCommand("mod mail block", context)}.
                    """));
            },
            Preconditions: [
                inGuild.Create(botMustBeInGuild: true),
                userHasPermission.Create(GuildPermission.BanMembers)
            ]
        ));
    }
}
