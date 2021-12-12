using Discord.Commands;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands
{
    public class LastFmUsernameTypeReader : TypeReader, ITaylorBotTypeReader
    {
        public Type ArgumentType => typeof(LastFmUsername);

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var result = LastFmUsernameParser.Parse(input);

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
