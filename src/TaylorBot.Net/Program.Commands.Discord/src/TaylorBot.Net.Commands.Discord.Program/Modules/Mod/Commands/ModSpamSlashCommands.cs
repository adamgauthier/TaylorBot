using Discord;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Commands;

public class ModSpamAddSlashCommand(
    ISpamChannelRepository spamChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission) : ISlashCommand<ModSpamAddSlashCommand.Options>
{
    public static string CommandName => "mod spam add";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedTextChannelOrCurrent channel);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var channel = options.channel.Channel;
                await spamChannelRepository.AddSpamChannelAsync(channel);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Users' messages and words in {options.channel.Channel.Mention} will no longer be counted. ✅
                    Use {context.MentionCommand("mod spam remove")} to revert and mark the channel as non-spam.
                    """));
            },
            Preconditions: [userHasPermission.Create(GuildPermission.ManageGuild)]
        ));
    }
}

public class ModSpamRemoveSlashCommand(
    ISpamChannelRepository spamChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission) : ISlashCommand<ModSpamRemoveSlashCommand.Options>
{
    public static string CommandName => "mod spam remove";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedTextChannelOrCurrent channel);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var channel = options.channel.Channel;
                await spamChannelRepository.RemoveSpamChannelAsync(channel);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Users' messages and words in {options.channel.Channel.Mention} are now counted. ✅
                    Use {context.MentionCommand("mod spam add")} to revert and mark the channel as spam.
                    """));
            },
            Preconditions: [userHasPermission.Create(GuildPermission.ManageGuild)]
        ));
    }
}
