using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Types
{
    public class MentionedUserNotAuthor<T> where T : class, IUser
    {
        public T User { get; }

        public MentionedUserNotAuthor(T user)
        {
            User = user;
        }
    }

    public class MentionedUserNotAuthorTypeReader<T> : TypeReader
        where T : class, IUser
    {
        private readonly MentionedUserTypeReader<T> _mentionedUserTypeReader;

        public MentionedUserNotAuthorTypeReader(MentionedUserTypeReader<T> mentionedUserTypeReader)
        {
            _mentionedUserTypeReader = mentionedUserTypeReader;
        }

        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var result = await _mentionedUserTypeReader.ReadAsync(context, input, services);
            if (result.Values != null)
            {
                var mentioned = (MentionedUser<T>)result.Values.Single().Value;
                if (mentioned.User == context.User)
                {
                    return TypeReaderResult.FromError(CommandError.ParseFailed, $"You can't mention yourself.");
                }
                return TypeReaderResult.FromSuccess(new MentionedUserNotAuthor<T>(mentioned.User));
            }
            else
            {
                return result;
            }
        }
    }
}
