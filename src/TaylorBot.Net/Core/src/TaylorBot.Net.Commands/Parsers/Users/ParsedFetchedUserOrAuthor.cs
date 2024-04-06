using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedFetchedUserOrAuthor(IUser User);

public class FetchedUserOrAuthorParser(FetchedUserParser userParser, ITaylorBotClient taylorBotClient) : IOptionParser<ParsedFetchedUserOrAuthor>
{
    public async ValueTask<Result<ParsedFetchedUserOrAuthor, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        if (optionValue.HasValue)
        {
            var parsedUser = await userParser.ParseAsync(context, optionValue, resolved);
            if (parsedUser)
            {
                return new ParsedFetchedUserOrAuthor(parsedUser.Value.User);
            }
            else
            {
                return Error(parsedUser.Error);
            }
        }
        else
        {
            var user = context.FetchedUser ?? await FetchUserAsync(context);
            return new ParsedFetchedUserOrAuthor(user);
        }
    }

    private async Task<IUser> FetchUserAsync(RunContext context)
    {
        return context.Guild != null
            ? await taylorBotClient.ResolveGuildUserAsync(context.Guild.Id, context.User.Id) ?? await taylorBotClient.ResolveRequiredUserAsync(context.User.Id)
            : await taylorBotClient.ResolveRequiredUserAsync(context.User.Id);
    }
}
