using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.EntityTracker.Domain;

namespace TaylorBot.Net.Commands.Types
{
    /// <summary>
    ///     A <see cref="TypeReader"/> for parsing objects implementing <see cref="IUser"/>.
    /// </summary>
    /// <typeparam name="T">The type to be checked; must implement <see cref="IUser"/>.</typeparam>
    public class CustomUserTypeReader<T> : TypeReader
        where T : class, IUser
    {
        /// <inheritdoc />
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var results = new Dictionary<ulong, TypeReaderValue>();
            var channelUsers = context.Channel.GetUsersAsync(CacheMode.CacheOnly).Flatten(); // it's better
            IReadOnlyCollection<IGuildUser> guildUsers = ImmutableArray.Create<IGuildUser>();

            if (context.Guild != null)
                guildUsers = await context.Guild.GetUsersAsync(CacheMode.CacheOnly).ConfigureAwait(false);

            //By Mention (1.0)
            if (MentionUtils.TryParseUser(input, out var id))
            {
                if (context.Guild != null)
                    AddResult(results, await context.Guild.GetUserAsync(id, CacheMode.AllowDownload).ConfigureAwait(false) as T, 1.00f);
                else
                    AddResult(results, await context.Channel.GetUserAsync(id, CacheMode.AllowDownload).ConfigureAwait(false) as T, 1.00f);
            }

            //By Id (0.9)
            if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out id))
            {
                if (context.Guild != null)
                    AddResult(results, await context.Guild.GetUserAsync(id, CacheMode.AllowDownload).ConfigureAwait(false) as T, 0.90f);
                else
                    AddResult(results, await context.Channel.GetUserAsync(id, CacheMode.AllowDownload).ConfigureAwait(false) as T, 0.90f);
            }

            //By Username + Discriminator (0.7-0.85)
            //By Discriminator (0.3-0.45)
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
                        AddResult(results, channelUser as T, channelUser.Username == username ? 0.85f : 0.75f);
                    foreach (var channelUser in channelUsernameMatches[false])
                        AddResult(results, channelUser as T, channelUser.Username == username ? 0.45f : 0.40f);

                    var guildUsernameMatches = await channelUsers.Where(x => x.DiscriminatorValue == discriminator)
                        .ToLookupAsync(u => string.Equals(username, u.Username, StringComparison.OrdinalIgnoreCase))
                        .ConfigureAwait(false);
                    foreach (var guildUser in guildUsernameMatches[true])
                        AddResult(results, guildUser as T, guildUser.Username == username ? 0.80f : 0.70f);
                    foreach (var guildUser in guildUsernameMatches[false])
                        AddResult(results, guildUser as T, guildUser.Username == username ? 0.35f : 0.30f);
                }
            }

            //By Username (0.2-0.65)
            {
                await channelUsers
                    .ForEachAsync(u =>
                    {
                        if (u.Username.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (string.Equals(input, u.Username, StringComparison.OrdinalIgnoreCase))
                            {
                                AddResult(results, u as T, u.Username == input ? 0.65f : 0.55f);
                            }
                            else
                            {
                                AddResult(results, u as T, 0.25f);
                            }
                        }
                    });


                foreach (var guildUser in guildUsers)
                {
                    if (guildUser.Username.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (string.Equals(input, guildUser.Username, StringComparison.OrdinalIgnoreCase))
                        {
                            AddResult(results, guildUser as T, guildUser.Username == input ? 0.60f : 0.50f);
                        }
                        else
                        {
                            AddResult(results, guildUser as T, 0.20f);
                        }
                    }
                }
            }

            //By Nickname (0.2-0.65)
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
                                AddResult(results, u as T, u.Nickname == input ? 0.65f : 0.55f);
                            }
                            else
                            {
                                AddResult(results, u as T, 0.25f);
                            }
                        }
                    });

                foreach (var guildUser in guildUsers.Where(u => u.Nickname != null))
                {
                    if (guildUser.Nickname.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (string.Equals(input, guildUser.Nickname, StringComparison.OrdinalIgnoreCase))
                        {
                            AddResult(results, guildUser as T, guildUser.Nickname == input ? 0.60f : 0.50f);
                        }
                        else
                        {
                            AddResult(results, guildUser as T, 0.20f);
                        }
                    }
                }
            }

            if (results.Count > 0)
            {
                var allMatches = results.Values.ToImmutableArray();
                var bestUser = (T)allMatches.OrderByDescending(y => y.Score).First().Value;

                var ignoredUserRepository = services.GetRequiredService<IIgnoredUserRepository>();
                var usernameTrackerDomainService = services.GetRequiredService<UsernameTrackerDomainService>();

                var getUserIgnoreUntilResult = await ignoredUserRepository.InsertOrGetUserIgnoreUntilAsync(bestUser);
                await usernameTrackerDomainService.AddUsernameAfterUserAddedAsync(bestUser, getUserIgnoreUntilResult);

                if (bestUser is IGuildUser bestGuildUser)
                {
                    var memberRepository = services.GetRequiredService<IMemberRepository>();

                    var memberAdded = await memberRepository.AddOrUpdateMemberAsync(bestGuildUser);

                    if (memberAdded)
                    {
                        services.GetRequiredService<ILogger<CustomUserTypeReader<T>>>().LogInformation(LogString.From(
                            $"Added new member {bestGuildUser.FormatLog()}."
                        ));
                    }
                }

                return TypeReaderResult.FromSuccess(allMatches);
            }

            return TypeReaderResult.FromError(CommandError.ObjectNotFound, $"Could not find user '{input}'. Mention with @ to make sure I find them.");
        }

        private void AddResult(Dictionary<ulong, TypeReaderValue> results, T user, float score)
        {
            if (user != null && !results.ContainsKey(user.Id))
                results.Add(user.Id, new TypeReaderValue(user, score));
        }
    }
}
