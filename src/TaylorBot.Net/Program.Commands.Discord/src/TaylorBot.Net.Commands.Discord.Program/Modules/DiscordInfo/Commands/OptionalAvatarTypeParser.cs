using System.Text.Json;
using System.Threading.Tasks;
using OperationResult;
using TaylorBot.Net.Commands.Parsers;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands
{
    public enum AvatarType
    {
        Guild,
        Global,
    }
    
    public class OptionalAvatarTypeParser : IOptionParser<AvatarType?>
    {
        public static Result<AvatarType, ParsingFailed> Parse(string input)
        {
            return input switch
            {
                "guild" => Ok(
                    AvatarType.Guild
                ),
                "global" => Ok(
                    AvatarType.Global
                ),
                _ => Error(new ParsingFailed(
                    $"Could not parse '{input}' into a valid Avatar type."
                )),
            };
        }

        public ValueTask<Result<AvatarType?, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            if (!optionValue.HasValue)
            {
                return new((AvatarType?)null);
            }

            var input = optionValue.Value.GetString()!;

            var result = Parse(input);

            return new(
                result.IsSuccess ? (AvatarType?)result.Value : Error(result.Error)
            );
        }
    }
}
