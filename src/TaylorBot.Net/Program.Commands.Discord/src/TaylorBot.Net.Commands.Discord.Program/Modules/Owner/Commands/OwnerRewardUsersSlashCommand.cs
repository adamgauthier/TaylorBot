using Discord;
using Discord.Net;
using Humanizer;
using System.Globalization;
using System.Text;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Owner.Commands;

public class OwnerRewardUsersSlashCommand(
    ITaypointRewardRepository taypointRepository,
    ITaylorBotClient client,
    IUserTracker userTracker,
    PageMessageFactory pageMessageFactory,
    TaylorBotOwnerPrecondition ownerPrecondition) : ISlashCommand<OwnerRewardUsersSlashCommand.Options>
{
    public static string CommandName => "owner reward-users";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedPositiveInteger amount, ParsedString users);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var tokens = options.users.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (tokens.Length == 0)
                {
                    return new EmbedResult(EmbedFactory.CreateError("Specify at least one user mention or user ID."));
                }

                var (users, rejected) = await ResolveRecipientsAsync(tokens);
                foreach (var user in users)
                {
                    await userTracker.TrackUserFromArgumentAsync(user);
                }

                var rewarded = users.Count > 0
                    ? await taypointRepository.RewardUsersAsync(users, options.amount.Value)
                    : [];
                var balances = rewarded.ToDictionary(user => user.UserId.Id);
                List<string> entries = [];
                foreach (var user in users)
                {
                    if (balances.TryGetValue(user.Id.Id, out var balance))
                    {
                        entries.Add(OwnerRewardMessage.FormatBalance(balance, options.amount.Value));
                    }
                    else
                    {
                        throw new InvalidOperationException($"Reward result is missing user '{user.Id}'.");
                    }
                }
                var header = OwnerRewardMessage.FormatHeader(context.User, options.amount.Value, rewarded);
                if (rejected.Count > 0)
                {
                    header += $"\nRejected {"recipient".ToQuantity(rejected.Count)}.";
                }
                var pages = CreatePages(header, entries, rejected);
                var embed = new EmbedBuilder()
                    .WithColor(rewarded.Count > 0 ? TaylorBotColors.SuccessColor : TaylorBotColors.ErrorColor);

                return pageMessageFactory.Create(new(
                    new(new EmbedDescriptionTextEditor(embed, pages, hasPageFooter: pages.Count > 1, emptyText: "No recipients."))));
            },
            Preconditions: [ownerPrecondition]
        ));
    }

    private async Task<(List<DiscordUser> Users, List<string> Rejected)> ResolveRecipientsAsync(IReadOnlyList<string> tokens)
    {
        HashSet<ulong> seenUsers = [];
        List<DiscordUser> users = [];
        List<string> rejected = [];
        foreach (var token in tokens.Distinct(StringComparer.Ordinal))
        {
            if (!TryParseUserId(token, out var id))
            {
                rejected.Add($"Rejected `{FormatRejectedToken(token)}`: invalid user mention or user ID.");
                continue;
            }
            if (!seenUsers.Add(id))
            {
                continue;
            }
            var user = await ResolveUserAsync(id);
            if (user == null)
            {
                rejected.Add($"Rejected `{id}`: user not found.");
                continue;
            }
            users.Add(new(user));
        }
        return (users, rejected);
    }

    private async Task<IUser?> ResolveUserAsync(ulong id)
    {
        var cached = await ((IDiscordClient)client.DiscordShardedClient).GetUserAsync(id, CacheMode.CacheOnly);
        if (cached != null)
        {
            return cached;
        }
        try
        {
            return await client.RestClient.GetUserAsync(id);
        }
        catch (HttpException exception) when (exception.DiscordCode == DiscordErrorCode.UnknownUser)
        {
            return null;
        }
    }

    private static bool TryParseUserId(string token, out ulong id)
    {
        if (MentionUtils.TryParseUser(token, out id))
        {
            return id != 0;
        }
        return ulong.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out id) && id != 0;
    }

    private static string FormatRejectedToken(string token) =>
        token.Replace('`', '\'').Replace("@", "(at)", StringComparison.Ordinal).Truncate(100);

    private static List<string> CreatePages(string header, IReadOnlyList<string> balances, IReadOnlyList<string> rejected)
    {
        List<string> pages = [];
        StringBuilder page = new(header);
        (string Title, IReadOnlyList<string> Entries)[] sections =
        [
            ("Balances Updated", balances),
            ("Rejected Recipients", rejected),
        ];
        foreach (var (title, entries) in sections)
        {
            var startsSection = true;
            foreach (var entry in entries)
            {
                var prefix = startsSection ? $"\n### {title}\n" : "\n";
                if (page.Length + prefix.Length + entry.Length > EmbedBuilder.MaxDescriptionLength)
                {
                    pages.Add(page.ToString());
                    page.Clear().Append(header);
                    prefix = $"\n### {title}\n";
                }
                page.Append(prefix).Append(entry);
                startsSection = false;
            }
        }
        pages.Add(page.ToString());
        return pages;
    }
}
