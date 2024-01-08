using Discord;
using Discord.Commands;
using System.Collections.Immutable;

namespace TaylorBot.Net.Commands.Types;

public class RoleNotEveryoneArgument<T>(T role) where T : class, IRole
{
    public T Role { get; } = role;
}

public class RoleNotEveryoneTypeReader<T>(CustomRoleTypeReader<T> customRoleTypeReader) : TypeReader
    where T : class, IRole
{
    public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        var result = await customRoleTypeReader.ReadAsync(context, input, services);
        if (result.Values != null)
        {
            var filteredValues = result.Values
                .Select(v => new { Score = v.Score, Value = (RoleArgument<T>)v.Value })
                .Where(v => v.Value.Role.Id != context.Guild.EveryoneRole.Id)
                .Select(v => new TypeReaderValue(new RoleNotEveryoneArgument<T>(v.Value.Role), v.Score))
                .ToImmutableArray();

            if (filteredValues.Length > 0)
            {
                return TypeReaderResult.FromSuccess(filteredValues);
            }
            else
            {
                return TypeReaderResult.FromError(CommandError.ObjectNotFound, $"Could not find role '{input}' in this server, the everyone role matches but can't be used.");
            }
        }
        else
        {
            return result;
        }
    }
}
