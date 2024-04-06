using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedMember(IGuildUser Member);

public class MemberParser(ITaylorBotClient taylorBotClient, IUserTracker userTracker) : IOptionParser<ParsedMember>
{
    public async ValueTask<Result<ParsedMember, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var option = optionValue?.GetString();
        if (option is null)
        {
            return Error(new ParsingFailed("Member option is required."));
        }
        if (context.Guild == null)
        {
            return Error(new ParsingFailed("Member option can only be used in a server."));
        }

        var member = context.Guild.Fetched != null
            ? await taylorBotClient.ResolveGuildUserAsync(context.Guild.Fetched, new(option))
            : await taylorBotClient.ResolveGuildUserAsync(context.Guild.Id, new(option));

        if (member == null)
        {
            return Error(new ParsingFailed("Did not find member in the current server."));
        }

        await userTracker.TrackUserFromArgumentAsync(new(member));

        return new ParsedMember(member);
    }
}
