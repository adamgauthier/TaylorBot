using Discord;
using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users
{
    public record ParsedUser(IUser User);

    public class UserParser : IOptionParser<ParsedUser>
    {
        private readonly ITaylorBotClient _taylorBotClient;
        private readonly IUserTracker _userTracker;

        public UserParser(ITaylorBotClient taylorBotClient, IUserTracker userTracker)
        {
            _taylorBotClient = taylorBotClient;
            _userTracker = userTracker;
        }

        public async ValueTask<Result<ParsedUser, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            if (!optionValue.HasValue)
            {
                return Error(new ParsingFailed("User option is required."));
            }

            var user = await _taylorBotClient.ResolveRequiredUserAsync(new(optionValue.Value.GetString()!));

            await _userTracker.TrackUserFromArgumentAsync(user);

            return new ParsedUser(user);
        }
    }
}
