using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Commands;

public class ModSpamAddSlashCommand : ISlashCommand<ModSpamAddSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("mod spam add");

    public record Options(ParsedNonThreadTextChannelOrCurrent channel);

    private readonly ISpamChannelRepository _spamChannelRepository;

    public ModSpamAddSlashCommand(ISpamChannelRepository spamChannelRepository)
    {
        _spamChannelRepository = spamChannelRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var channel = options.channel.Channel;
                await _spamChannelRepository.AddSpamChannelAsync(channel);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""                    
                    Users' messages and words in {options.channel.Channel.Mention} will no longer be counted. ✅
                    Use {context.MentionCommand("mod spam remove")} to revert and mark the channel as non-spam.
                    """));
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild),
            }
        ));
    }
}

public class ModSpamRemoveSlashCommand : ISlashCommand<ModSpamRemoveSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("mod spam remove");

    public record Options(ParsedNonThreadTextChannelOrCurrent channel);

    private readonly ISpamChannelRepository _spamChannelRepository;

    public ModSpamRemoveSlashCommand(ISpamChannelRepository spamChannelRepository)
    {
        _spamChannelRepository = spamChannelRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var channel = options.channel.Channel;
                await _spamChannelRepository.RemoveSpamChannelAsync(channel);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""                    
                    Users' messages and words in {options.channel.Channel.Mention} will no longer be counted. ✅
                    Use {context.MentionCommand("mod spam add")} to revert and mark the channel as spam.
                    """));
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild),
            }
        ));
    }
}
