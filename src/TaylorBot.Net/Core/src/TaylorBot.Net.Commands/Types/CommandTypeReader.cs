using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace TaylorBot.Net.Commands.Types;

public class CommandTypeReader : TypeReader
{
    public Type ArgumentType => typeof(ICommandRepository.Command);

    public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        var sanitized = input.Trim().ToLowerInvariant();

        var commandRepository = services.GetRequiredService<ICommandRepository>();

        var command = await commandRepository.FindCommandByAliasAsync(sanitized);

        if (command != null)
        {
            return TypeReaderResult.FromSuccess(command);
        }
        else
        {
            return TypeReaderResult.FromError(
                CommandError.ParseFailed, $"Could not find command '{input}'."
            );
        }
    }
}
