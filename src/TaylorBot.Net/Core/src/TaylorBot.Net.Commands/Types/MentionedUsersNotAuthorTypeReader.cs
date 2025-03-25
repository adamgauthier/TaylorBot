using Discord;
using Discord.Commands;

namespace TaylorBot.Net.Commands.Types;

public class MentionedUsersNotAuthorTypeReader<T>(MentionedUserNotAuthorTypeReader<T> mentionedUserNotAuthorTypeReader) : TypeReader
    where T : class, IUser
{
    private const int MaxMentionCount = 25;

    public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        var split = input
            .Split(' ')
            .Select(u => u.Trim())
            .Where(i => i.Length > 0)
            .ToList();

        var results = new List<IMentionedUserNotAuthor<T>>();

        foreach (var mention in split)
        {
            var result = await mentionedUserNotAuthorTypeReader.ReadAsync(context, mention, services);
            if (result.Values != null)
            {
                results.Add((IMentionedUserNotAuthor<T>)result.BestMatch);
            }
            else
            {
                return result;
            }
        }

        var distinct = results.DistinctBy(r => r.UserId).ToList();

        if (distinct.Count > MaxMentionCount)
        {
            return TypeReaderResult.FromError(CommandError.ParseFailed, $"You can't mention more than {MaxMentionCount} people.");
        }

        return TypeReaderResult.FromSuccess(distinct);
    }
}
