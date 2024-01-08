using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace TaylorBot.Net.Commands.Types;

public interface IConstrainedInt
{
    int Parsed { get; }
}

public interface IConstrainedIntFactory
{
    IConstrainedInt Create(int value);
}

public class ConstrainedIntTypeReader<T>(int minimumInclusive, int? maximumInclusive = null) : TypeReader
    where T : IConstrainedIntFactory
{
    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        if (int.TryParse(input, out var parsed))
        {
            if (parsed < minimumInclusive)
            {
                return Task.FromResult(TypeReaderResult.FromError(
                    CommandError.ParseFailed, $"Number '{input}' must be equal to or greater than {minimumInclusive}."
                ));
            }

            if (maximumInclusive.HasValue && parsed > maximumInclusive.Value)
            {
                return Task.FromResult(TypeReaderResult.FromError(
                    CommandError.ParseFailed, $"Number '{input}' must be equal to or less than {maximumInclusive}."
                ));
            }

            var factory = services.GetRequiredService<T>();

            return Task.FromResult(TypeReaderResult.FromSuccess(
                factory.Create(parsed)
            ));
        }
        else
        {
            return Task.FromResult(TypeReaderResult.FromError(
                CommandError.ParseFailed, $"Could not parse '{input}' into a valid number."
            ));
        }
    }
}
