using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Types;

public interface IMentionedUserNotAuthorOrClient<T> : IUserArgument<T> where T : class, IUser { }

public class MentionedUserNotAuthorOrClientTypeReader<T>(MentionedUserNotAuthorTypeReader<T> mentionedUserNotAuthorTypeReader) : TypeReader
    where T : class, IUser
{
    public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        var result = await mentionedUserNotAuthorTypeReader.ReadAsync(context, input, services);
        if (result.Values != null)
        {
            var mentioned = (IUserArgument<T>)result.BestMatch;
            if (mentioned.UserId == ((ITaylorBotCommandContext)context).CurrentUser.Id)
            {
                return TypeReaderResult.FromError(CommandError.ParseFailed, $"You can't mention me.");
            }
            return TypeReaderResult.FromSuccess(mentioned);
        }
        else
        {
            return result;
        }
    }
}
