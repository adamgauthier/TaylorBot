using Discord;
using Discord.Commands;

namespace TaylorBot.Net.Commands.Types;

public class MentionedUsersNotAuthorTypeReader<T> : TypeReader
    where T : class, IUser
{
    private readonly MentionedUserNotAuthorTypeReader<T> _mentionedUserNotAuthorTypeReader;

    public MentionedUsersNotAuthorTypeReader(MentionedUserNotAuthorTypeReader<T> mentionedUserNotAuthorTypeReader)
    {
        _mentionedUserNotAuthorTypeReader = mentionedUserNotAuthorTypeReader;
    }

    public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        var split = input.Split(' ').Where(i => i != string.Empty);

        var results = new List<IMentionedUserNotAuthor<T>>();

        foreach (var mention in split)
        {
            var result = await _mentionedUserNotAuthorTypeReader.ReadAsync(context, mention, services);
            if (result.Values != null)
            {
                results.Add((IMentionedUserNotAuthor<T>)result.BestMatch);
            }
            else
            {
                return result;
            }
        }

        var distinct = results.GroupBy(r => r.UserId).Select(g => g.First());

        return TypeReaderResult.FromSuccess(distinct.ToList());
    }
}
