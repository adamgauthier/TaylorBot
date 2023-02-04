using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Valentines.Domain;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Valentines.Commands
{
    public class LoveHistorySlashCommand : ISlashCommand<LoveHistorySlashCommand.Options>
    {
        public SlashCommandInfo Info => new("love history");

        public record Options(ParsedUserOrAuthor user);

        private readonly IValentinesRepository _valentinesRepository;

        public LoveHistorySlashCommand(IValentinesRepository valentinesRepository)
        {
            _valentinesRepository = valentinesRepository;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new Command(
                new(Info.Name),
                async () =>
                {
                    var config = await _valentinesRepository.GetConfigurationAsync();
                    IGuildUser member = (IGuildUser)options.user.User;

                    var allObtained = await _valentinesRepository.GetAllAsync();

                    if (!allObtained.Any())
                    {
                        return new EmbedResult(EmbedFactory.CreateError("No love spreading data ☹️"));
                    }

                    var givenTo = allObtained.ToDictionary(o => o.ToUserId.Id);

                    if (givenTo.TryGetValue(member.Id, out var targetUserReceived))
                    {
                        List<RoleObtained> chain = new() { targetUserReceived };
                        BuildChain(givenTo, chain, targetUserReceived);
                        chain.Reverse();

                        var obtainedAsLines = chain.Select(o => $"{o.AcquiredAt:MMM d}: **{o.FromName}** 💌➡️ **{o.ToUserName}**");

                        var pages =
                            obtainedAsLines.Chunk(size: 15)
                            .Select(lines => string.Join('\n', lines))
                            .ToList();

                        return new PageMessageResultBuilder(new(
                            new(new EmbedDescriptionTextEditor(
                                new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor).WithUserAsAuthor(member),
                                pages,
                                hasPageFooter: true,
                                emptyText: "No love history. 🤔"
                            ))
                        )).Build();
                    }
                    else
                    {
                        return new EmbedResult(EmbedFactory.CreateError(
                            $"{member.Mention} has never received the {MentionUtils.MentionRole(config.SpreadLoveRoleId.Id)} role. 😭"
                        ));
                    }
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                }
            ));
        }

        private void BuildChain(Dictionary<ulong, RoleObtained> givenTo, List<RoleObtained> chain, RoleObtained end)
        {
            if (end.FromUserId == end.ToUserId)
            {
                return;
            }

            var given = givenTo[end.FromUserId.Id];
            chain.Add(given);
            BuildChain(givenTo, chain, given);
        }
    }
}
