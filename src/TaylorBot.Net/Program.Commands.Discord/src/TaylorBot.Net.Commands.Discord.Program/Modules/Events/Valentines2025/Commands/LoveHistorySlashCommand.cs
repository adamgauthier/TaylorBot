using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2025.Domain;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2025.Commands;

public class LoveHistorySlashCommand(
    IValentinesRepository valentinesRepository,
    InGuildPrecondition.Factory inGuild,
    PageMessageFactory pageMessageFactory) : ISlashCommand<LoveHistorySlashCommand.Options>
{
    public static string CommandName => "love history";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var config = await valentinesRepository.GetConfigurationAsync();
                var user = options.user.User;

                var allObtained = await valentinesRepository.GetAllAsync();

                if (!allObtained.Any())
                {
                    return new EmbedResult(EmbedFactory.CreateError("No love spreading data 😕"));
                }

                var givenTo = allObtained.ToDictionary(o => o.ToUserId.Id);

                if (givenTo.TryGetValue(user.Id, out var targetUserReceived))
                {
                    List<RoleObtained> chain = [targetUserReceived];
                    BuildChain(givenTo, chain, targetUserReceived);
                    chain.Reverse();

                    var obtainedAsLines = chain.Select(o => $"{o.AcquiredAt:MMM d}: **{o.FromName}** 💌➡️ **{o.ToUserName}**");

                    var pages =
                        obtainedAsLines.Chunk(size: 15)
                        .Select(lines => string.Join('\n', lines))
                        .ToList();

                    return pageMessageFactory.Create(new(
                        new(new EmbedDescriptionTextEditor(
                            new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor).WithUserAsAuthor(user),
                            pages,
                            hasPageFooter: true,
                            emptyText: "No love history 🤔"
                        ))
                    ));
                }
                else
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"{user.Mention} has never received the {MentionUtils.MentionRole(config.SpreadLoveRoleId.Id)} role 😭"
                    ));
                }
            },
            Preconditions: [
                inGuild.Create(botMustBeInGuild: true),
            ]
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
