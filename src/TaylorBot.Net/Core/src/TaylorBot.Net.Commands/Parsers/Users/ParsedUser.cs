using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedUser(IUser User);

public class UserParser(ITaylorBotClient taylorBotClient, IUserTracker userTracker) : IOptionParser<ParsedUser>
{
    public async ValueTask<Result<ParsedUser, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        if (!optionValue.HasValue)
        {
            return Error(new ParsingFailed("User option is required."));
        }

        SnowflakeId userId = new(optionValue.Value.GetString()!);
        var user = context.Guild != null
            ? await taylorBotClient.ResolveGuildUserAsync(context.Guild, userId) ??
              await taylorBotClient.ResolveRequiredUserAsync(userId)
            : await taylorBotClient.ResolveRequiredUserAsync(userId);

        await userTracker.TrackUserFromArgumentAsync(user);

        return new ParsedUser(user);
    }
}
