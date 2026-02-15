using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2026.Domain;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2026.Commands;

public class LoveLeaderboardSlashCommand(
    IValentinesRepository valentinesRepository,
    InGuildPrecondition.Factory inGuild,
    PageMessageFactory pageMessageFactory) : ISlashCommand<NoOptions>
{
    public static string CommandName => "love leaderboard";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var allObtained = await valentinesRepository.GetAllAsync();

                if (!allObtained.Any())
                {
                    return new EmbedResult(EmbedFactory.CreateError("No love spreading data 😕"));
                }

                var givenTo = allObtained.ToDictionary(o => o.ToUserId.Id);
                Dictionary<ulong, List<ulong>> children = [];

                foreach (var r in allObtained)
                {
                    if (!children.ContainsKey(r.FromUserId.Id))
                        children[r.FromUserId.Id] = [];
                    children[r.FromUserId.Id].Add(r.ToUserId.Id);
                }

                // Leaves = users who haven't spread love to anyone
                var leaves = allObtained
                    .Select(r => r.ToUserId.Id)
                    .Where(id => !children.ContainsKey(id) || children[id].Count == 0)
                    .ToList();

                List<(string ChainStartedBy, string CurrentHolder, int ChainLength)> chains = [];

                foreach (var leafId in leaves)
                {
                    List<RoleObtained> path = [givenTo[leafId]];
                    var current = givenTo[leafId];
                    while (current.FromUserId != current.ToUserId)
                    {
                        current = givenTo[current.FromUserId.Id];
                        path.Add(current);
                    }
                    path.Reverse();

                    // path[0] is the self-give (adam), path[1] is the chain originator
                    var originator = path.Count > 1 ? path[1].ToUserName : path[0].ToUserName;
                    var currentHolder = path[^1].ToUserName;
                    var chainLength = path.Count - 1; // Exclude the self-give
                    chains.Add((originator, currentHolder, chainLength));
                }

                chains = [.. chains.OrderByDescending(c => c.ChainLength)];

                var lines = chains.Select((c, i) =>
                    $"{i + 1}\\. **{c.ChainStartedBy}** ➡️ **{c.CurrentHolder}** — 💌 {c.ChainLength} spread{(c.ChainLength != 1 ? "s" : "")}"
                );

                var pages = lines
                    .Chunk(15)
                    .Select(chunk => string.Join('\n', chunk))
                    .ToList();

                return pageMessageFactory.Create(new(
                    new(new EmbedDescriptionTextEditor(
                        new EmbedBuilder()
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithTitle("Love Chain Leaderboard 💌"),
                        pages,
                        hasPageFooter: true,
                        emptyText: "No love chains yet 🤔"
                    )),
                    IsCancellable: true
                ));
            },
            Preconditions: [
                inGuild.Create(botMustBeInGuild: true),
            ]
        ));
    }
}
