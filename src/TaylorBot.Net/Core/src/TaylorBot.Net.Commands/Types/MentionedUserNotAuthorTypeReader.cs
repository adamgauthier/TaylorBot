using Discord;
using Discord.Commands;

namespace TaylorBot.Net.Commands.Types;

public interface IMentionedUserNotAuthor<T> : IUserArgument<T> where T : class, IUser { }

public class MentionedUserNotAuthorTypeReader<T>(MentionedUserTypeReader<T> mentionedUserTypeReader) : TypeReader
    where T : class, IUser
{
    public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        var result = await mentionedUserTypeReader.ReadAsync(context, input, services);
        if (result.Values != null)
        {
            var mentioned = (IUserArgument<T>)result.BestMatch;
            if (mentioned.UserId == context.User.Id)
            {
                return TypeReaderResult.FromError(CommandError.ParseFailed, "You can't mention yourself.");
            }
            return TypeReaderResult.FromSuccess(mentioned);
        }
        else
        {
            return result;
        }
    }
}
