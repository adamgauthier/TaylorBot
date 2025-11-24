using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Types;

public class CustomUserTypeReader<T>(MentionedUserTypeReader<T> mentionedUserTypeReader, IUserTracker userTracker) : TypeReader
    where T : class, IUser
{
    private sealed record UserVal<U>(IUserArgument<U> UserArgument, float Score) where U : class, IUser;

    private void AddResultIfTypeMatches(Dictionary<ulong, UserVal<T>> results, IUser user, float score)
    {
        if (user is T casted)
        {
            if (results.TryGetValue(user.Id, out var currentVal))
            {
                if (score > currentVal.Score)
                {
                    results[casted.Id] = new UserVal<T>(
                        new UserArgument<T>(casted, userTracker),
                        score
                    );
                }
            }
            else
            {
                results.Add(casted.Id, new UserVal<T>(
                    new UserArgument<T>(casted, userTracker),
                    score
                ));
            }
        }
    }

    private static IAsyncEnumerable<TElement> Flatten<TElement>(IAsyncEnumerable<IEnumerable<TElement>> source)
    {
        return AsyncEnumerable.SelectMany(source, AsyncEnumerable.ToAsyncEnumerable);
    }

    public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        var results = new Dictionary<ulong, UserVal<T>>();

        var channelUsers = Flatten(context.Channel.GetUsersAsync(CacheMode.CacheOnly));

        IReadOnlyCollection<IGuildUser> guildUsers = context.Guild != null ?
            await context.Guild.GetUsersAsync(CacheMode.CacheOnly).ConfigureAwait(false) :
            [];


        // By Mention (1.0)
        var mentionned = await mentionedUserTypeReader.ReadAsync(context, input, services);
        if (mentionned.Values != null)
        {
            var result = (IUserArgument<T>)mentionned.BestMatch;
            results.Add(result.UserId, new UserVal<T>(result, 1.00f));
        }

        // By Id (0.9)
        if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
        {
            var user = context.Guild != null ?
                await services.GetRequiredService<ITaylorBotClient>().ResolveGuildUserAsync(context.Guild, new SnowflakeId(id)) :
                await context.Channel.GetUserAsync(id, CacheMode.CacheOnly).ConfigureAwait(false);

            if (user != null)
                AddResultIfTypeMatches(results, user, 0.90f);
        }

        // By Username + Discriminator (0.7-0.85)
        // By Discriminator (0.3-0.45)
        var index = input.LastIndexOf('#');
        if (index >= 0)
        {
            var username = input[..index];
            if (ushort.TryParse(input.AsSpan(index + 1), out var discriminator))
            {
                var channelUsernameMatches = await channelUsers.Where(u => u.DiscriminatorValue == discriminator)
                    .ToLookupAsync(u => string.Equals(username, u.Username, StringComparison.OrdinalIgnoreCase))
                    .ConfigureAwait(false);

                foreach (var channelUser in channelUsernameMatches[true])
                    AddResultIfTypeMatches(results, channelUser, channelUser.Username == username ? 0.85f : 0.75f);
                foreach (var channelUser in channelUsernameMatches[false])
                    AddResultIfTypeMatches(results, channelUser, channelUser.Username == username ? 0.45f : 0.40f);


                var guildUsernameMatches = guildUsers.Where(u => u.DiscriminatorValue == discriminator)
                    .ToLookup(u => string.Equals(username, u.Username, StringComparison.OrdinalIgnoreCase));

                foreach (var guildUser in guildUsernameMatches[true])
                    AddResultIfTypeMatches(results, guildUser, guildUser.Username == username ? 0.80f : 0.70f);
                foreach (var guildUser in guildUsernameMatches[false])
                    AddResultIfTypeMatches(results, guildUser, guildUser.Username == username ? 0.35f : 0.30f);
            }
        }

        // By Username (0.1-0.65)
        {
            await foreach (var u in channelUsers)
            {
                if (u.Username.Contains(input, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(input, u.Username, StringComparison.OrdinalIgnoreCase))
                    {
                        AddResultIfTypeMatches(results, u, u.Username == input ? 0.65f : 0.55f);
                    }
                    else if (u.Username.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                    {
                        AddResultIfTypeMatches(results, u, 0.25f);
                    }
                    else
                    {
                        AddResultIfTypeMatches(results, u, 0.20f);
                    }
                }
            }

            foreach (var guildUser in guildUsers)
            {
                if (guildUser.Username.Contains(input, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(input, guildUser.Username, StringComparison.OrdinalIgnoreCase))
                    {
                        AddResultIfTypeMatches(results, guildUser, guildUser.Username == input ? 0.60f : 0.50f);
                    }
                    else if (guildUser.Username.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                    {
                        AddResultIfTypeMatches(results, guildUser, 0.15f);
                    }
                    else
                    {
                        AddResultIfTypeMatches(results, guildUser, 0.10f);
                    }
                }
            }
        }

        // By Nickname (0.1-0.65)
        {
            var guildUsersWithNickname = channelUsers
                .OfType<IGuildUser>()
                .Where(u => u.Nickname != null);

            await foreach (var u in guildUsersWithNickname)
            {
                if (u.Nickname.Contains(input, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(input, u.Nickname, StringComparison.OrdinalIgnoreCase))
                    {
                        AddResultIfTypeMatches(results, u, u.Nickname == input ? 0.65f : 0.55f);
                    }
                    else if (u.Nickname.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                    {
                        AddResultIfTypeMatches(results, u, 0.25f);
                    }
                    else
                    {
                        AddResultIfTypeMatches(results, u, 0.20f);
                    }
                }
            }

            foreach (var guildUser in guildUsers.Where(u => u.Nickname != null))
            {
                if (guildUser.Nickname.Contains(input, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(input, guildUser.Nickname, StringComparison.OrdinalIgnoreCase))
                    {
                        AddResultIfTypeMatches(results, guildUser, guildUser.Nickname == input ? 0.60f : 0.50f);
                    }
                    else if (guildUser.Nickname.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                    {
                        AddResultIfTypeMatches(results, guildUser, 0.15f);
                    }
                    else
                    {
                        AddResultIfTypeMatches(results, guildUser, 0.10f);
                    }
                }
            }
        }

        if (results.Count > 0)
        {
            return TypeReaderResult.FromSuccess(
                [.. results.Values.Select(u => new TypeReaderValue(u.UserArgument, u.Score))]
            );
        }

        return TypeReaderResult.FromError(CommandError.ObjectNotFound, $"Could not find user '{input}'. Mention with @ to make sure I find them.");
    }
}
