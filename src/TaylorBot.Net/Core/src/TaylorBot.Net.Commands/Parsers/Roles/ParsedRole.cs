using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Roles;

public record ParsedRole(DiscordRole Role);

public class RoleParser() : IOptionParser<ParsedRole>
{
    public ValueTask<Result<ParsedRole, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var option = optionValue?.GetString();
        if (option == null)
        {
            return new(Error(new ParsingFailed("Role option is required 🤔")));
        }

        SnowflakeId roleId = new(option);

        if (resolved?.roles?.TryGetValue(roleId, out var resolvedRole) == true)
        {
            return new(new ParsedRole(resolvedRole));
        }
        else
        {
            throw new InvalidOperationException($"Can't find {option} in resolved data {JsonSerializer.Serialize(resolved)}");
        }
    }
}
