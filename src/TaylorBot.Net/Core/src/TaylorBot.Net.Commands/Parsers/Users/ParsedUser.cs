using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.User;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedUser(DiscordUser User);

public class UserParser(InteractionMapper interactionMapper, IUserTracker userTracker) : IOptionParser<ParsedUser>
{
    public async ValueTask<Result<ParsedUser, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var option = optionValue?.GetString();
        if (option == null)
        {
            return Error(new ParsingFailed("User option is required 🤔"));
        }

        if (resolved?.users?.TryGetValue(option, out var resolvedUser) == true)
        {
            DiscordUser user = new(
                resolvedUser.id,
                resolvedUser.username,
                resolvedUser.avatar,
                resolvedUser.discriminator,
                IsBot: resolvedUser.bot == true,
                MemberInfo: GetMemberInfo(context, resolved, option));

            // Command user is already tracked
            if (user.Id != context.User.Id && context.Guild?.Fetched != null)
            {
                await userTracker.TrackUserFromArgumentAsync(user);
            }

            return new ParsedUser(user);
        }
        else
        {
            throw new InvalidOperationException($"Can't find {option} in resolved data {JsonSerializer.Serialize(resolved)}");
        }
    }

    private DiscordMemberInfo? GetMemberInfo(RunContext context, Interaction.Resolved resolved, string option)
    {
        DiscordMemberInfo? memberInfo = null;

        if (context.Guild != null && resolved.members?.TryGetValue(option, out var resolvedMember) == true)
        {
            memberInfo = interactionMapper.ToMemberInfo(context.Guild.Id, resolvedMember);
        }

        return memberInfo;
    }
}
