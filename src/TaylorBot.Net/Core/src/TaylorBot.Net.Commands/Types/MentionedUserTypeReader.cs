using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Types
{
    public interface IMentionedUser<T> : IUserArgument<T> where T : class, IUser { }

    public class MentionedUserTypeReader<T> : TypeReader
        where T : class, IUser
    {
        private readonly IUserTracker _userTracker;

        public MentionedUserTypeReader(IUserTracker userTracker)
        {
            _userTracker = userTracker;
        }

        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (MentionUtils.TryParseUser(input, out var id))
            {
                var user = context.Guild != null ?
                    (T?)await services.GetRequiredService<ITaylorBotClient>().ResolveGuildUserAsync(context.Guild, new SnowflakeId(id)) :
                    (T?)await context.Channel.GetUserAsync(id, CacheMode.CacheOnly).ConfigureAwait(false);

                if (user == null)
                    return TypeReaderResult.FromError(CommandError.ParseFailed, $"Could not find user '{input}'.");

                return TypeReaderResult.FromSuccess(new UserArgument<T>(user, _userTracker));
            }
            else
            {
                return TypeReaderResult.FromError(CommandError.ParseFailed, $"You must mention the user directly with @. ('{input}')");
            }
        }
    }
}
