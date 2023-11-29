using Discord.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands
{
    public class LastFmPeriodTypeReader : TypeReader, ITaylorBotTypeReader
    {
        public Type ArgumentType => typeof(LastFmPeriod);

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var result = OptionalLastFmPeriodParser.Parse(input);

            if (result.IsSuccess)
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(
                    result.Value
                ));
            }
            else
            {
                return Task.FromResult(TypeReaderResult.FromError(
                    CommandError.ParseFailed, result.Error.Message
                ));
            }
        }
    }
}
