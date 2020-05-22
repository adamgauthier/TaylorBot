using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Types
{
    public class CustomUserTypeReader<T> : TypeReader
        where T : class, IUser
    {
        private readonly MentionedUserTypeReader<T> _mentionedUserTypeReader;
        private readonly IUserTracker _userTracker;

        public CustomUserTypeReader(MentionedUserTypeReader<T> mentionedUserTypeReader, IUserTracker userTracker)
        {
            _mentionedUserTypeReader = mentionedUserTypeReader;
            _userTracker = userTracker;
        }

        private class UserVal<U> where U : class, IUser
        {
            public IUserArgument<U> UserArgument { get; }
            public float Score { get; }

            public UserVal(IUserArgument<U> userArgument, float score)
            {
                UserArgument = userArgument;
                Score = score;
            }
        }

        private void AddResultIfTypeMatches(Dictionary<ulong, UserVal<T>> results, IUser user, float score)
        {
            if (user is T casted && !results.ContainsKey(user.Id))
            {
                results.Add(casted.Id, new UserVal<T>(
                    new UserArgument<T>(casted, _userTracker),
                    score
                ));
            }
        }

        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var results = new Dictionary<ulong, UserVal<T>>();

            var channelUsers = context.Channel.GetUsersAsync(CacheMode.CacheOnly).Flatten();

            IReadOnlyCollection<IGuildUser> guildUsers = context.Guild != null ?
                await context.Guild.GetUsersAsync(CacheMode.CacheOnly).ConfigureAwait(false) :
                ImmutableArray.Create<IGuildUser>();


            // By Mention (1.0)
            var mentionned = await _mentionedUserTypeReader.ReadAsync(context, input, services);
            if (mentionned.Values != null)
            {
                var result = (IUserArgument<T>)mentionned.Values.Single().Value;
                results.Add(result.UserId, new UserVal<T>(result, 1.00f));
            }

            // By Id (0.9)
            if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
            {
                var user = context.Guild != null ?
                    await context.Guild.GetUserAsync(id, CacheMode.AllowDownload).ConfigureAwait(false) :
                    await context.Channel.GetUserAsync(id, CacheMode.AllowDownload).ConfigureAwait(false);

                AddResultIfTypeMatches(results, user, 0.90f);
            }

            // By Username + Discriminator (0.7-0.85)
            // By Discriminator (0.3-0.45)
            int index = input.LastIndexOf('#');
            if (index >= 0)
            {
                string username = input.Substring(0, index);
                if (ushort.TryParse(input.Substring(index + 1), out ushort discriminator))
                {
                    var channelUsernameMatches = await channelUsers.Where(x => x.DiscriminatorValue == discriminator)
                        .ToLookupAsync(u => string.Equals(username, u.Username, StringComparison.OrdinalIgnoreCase))
                        .ConfigureAwait(false);

                    foreach (var channelUser in channelUsernameMatches[true])
                        AddResultIfTypeMatches(results, channelUser, channelUser.Username == username ? 0.85f : 0.75f);
                    foreach (var channelUser in channelUsernameMatches[false])
                        AddResultIfTypeMatches(results, channelUser, channelUser.Username == username ? 0.45f : 0.40f);


                    var guildUsernameMatches = await channelUsers.Where(x => x.DiscriminatorValue == discriminator)
                        .ToLookupAsync(u => string.Equals(username, u.Username, StringComparison.OrdinalIgnoreCase))
                        .ConfigureAwait(false);

                    foreach (var guildUser in guildUsernameMatches[true])
                        AddResultIfTypeMatches(results, guildUser, guildUser.Username == username ? 0.80f : 0.70f);
                    foreach (var guildUser in guildUsernameMatches[false])
                        AddResultIfTypeMatches(results, guildUser, guildUser.Username == username ? 0.35f : 0.30f);
                }
            }

            // By Username (0.2-0.65)
            {
                await channelUsers
                    .ForEachAsync(u =>
                    {
                        if (u.Username.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (string.Equals(input, u.Username, StringComparison.OrdinalIgnoreCase))
                            {
                                AddResultIfTypeMatches(results, u, u.Username == input ? 0.65f : 0.55f);
                            }
                            else
                            {
                                AddResultIfTypeMatches(results, u, 0.25f);
                            }
                        }
                    });


                foreach (var guildUser in guildUsers)
                {
                    if (guildUser.Username.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (string.Equals(input, guildUser.Username, StringComparison.OrdinalIgnoreCase))
                        {
                            AddResultIfTypeMatches(results, guildUser, guildUser.Username == input ? 0.60f : 0.50f);
                        }
                        else
                        {
                            AddResultIfTypeMatches(results, guildUser, 0.20f);
                        }
                    }
                }
            }

            // By Nickname (0.2-0.65)
            {
                await channelUsers
                    .OfType<IGuildUser>()
                    .Where(u => u.Nickname != null)
                    .ForEachAsync(u =>
                    {
                        if (u.Nickname.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (string.Equals(input, u.Nickname, StringComparison.OrdinalIgnoreCase))
                            {
                                AddResultIfTypeMatches(results, u, u.Nickname == input ? 0.65f : 0.55f);
                            }
                            else
                            {
                                AddResultIfTypeMatches(results, u, 0.25f);
                            }
                        }
                    });

                foreach (var guildUser in guildUsers.Where(u => u.Nickname != null))
                {
                    if (guildUser.Nickname.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (string.Equals(input, guildUser.Nickname, StringComparison.OrdinalIgnoreCase))
                        {
                            AddResultIfTypeMatches(results, guildUser, guildUser.Nickname == input ? 0.60f : 0.50f);
                        }
                        else
                        {
                            AddResultIfTypeMatches(results, guildUser, 0.20f);
                        }
                    }
                }
            }

            if (results.Count > 0)
            {
                return TypeReaderResult.FromSuccess(
                    results.Values.Select(u => new TypeReaderValue(u.UserArgument, u.Score)).ToImmutableArray()
                );
            }

            return TypeReaderResult.FromError(CommandError.ObjectNotFound, $"Could not find user '{input}'. Mention with @ to make sure I find them.");
        }
    }
}
