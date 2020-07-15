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
    public class RoleArgument<T> where T : class, IRole
    {
        public T Role { get; }

        public RoleArgument(T role) => Role = role;
    }

    public class CustomRoleTypeReader<T> : TypeReader
        where T : class, IRole
    {
        private class RoleVal<U> where U : class, IRole
        {
            public U Role { get; }
            public float Score { get; }

            public RoleVal(U role, float score)
            {
                Role = role;
                Score = score;
            }
        }

        private void AddResultIfTypeMatches(Dictionary<ulong, RoleVal<T>> results, IRole role, float score)
        {
            if (role is T casted && !results.ContainsKey(role.Id))
            {
                results.Add(casted.Id, new RoleVal<T>(
                    casted,
                    score
                ));
            }
        }

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (context.Guild != null)
            {
                var results = new Dictionary<ulong, RoleVal<T>>();
                var roles = context.Guild.Roles;

                // By Mention (1.0)
                if (MentionUtils.TryParseRole(input, out var id))
                    AddResultIfTypeMatches(results, context.Guild.GetRole(id), 1.00f);

                // By Id (0.9)
                if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out id))
                    AddResultIfTypeMatches(results, context.Guild.GetRole(id), 0.90f);

                // By Name (0.5-0.8)
                foreach (var role in roles)
                {
                    if (role.Name.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (string.Equals(input, role.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            AddResultIfTypeMatches(results, role, role.Name == input ? 0.80f : 0.70f);
                        }
                        else if (role.Name.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                        {
                            AddResultIfTypeMatches(results, role, 0.60f);
                        }
                        else
                        {
                            AddResultIfTypeMatches(results, role, 0.50f);
                        }
                    }
                }

                if (results.Count > 0)
                {
                    return Task.FromResult(TypeReaderResult.FromSuccess(
                        results.Values.Select(r => new TypeReaderValue(new RoleArgument<T>(r.Role), r.Score)).ToImmutableArray()
                    ));
                }
            }

            return Task.FromResult(
                TypeReaderResult.FromError(CommandError.ObjectNotFound, $"Could not find role '{input}' in this server.")
            );
        }
    }
}
