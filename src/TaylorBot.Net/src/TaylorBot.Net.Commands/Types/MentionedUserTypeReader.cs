using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Types
{
    public class MentionedUser<T> where T : class, IUser
    {
        public T User { get; }

        public MentionedUser(T user)
        {
            User = user;
        }
    }

    public class MentionedUserTypeReader<T> : TypeReader
        where T : class, IUser
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (MentionUtils.TryParseUser(input, out var id))
            {
                var user = context.Guild != null ?
                    (T)await context.Guild.GetUserAsync(id, CacheMode.AllowDownload).ConfigureAwait(false) :
                    (T)await context.Channel.GetUserAsync(id, CacheMode.AllowDownload).ConfigureAwait(false);

                return TypeReaderResult.FromSuccess(new MentionedUser<T>(user));
            }
            else
            {
                return TypeReaderResult.FromError(CommandError.ParseFailed, $"You must mention the user directly with @.");
            }
        }
    }
}
