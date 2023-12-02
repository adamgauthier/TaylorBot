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

public class ConstrainedIntTypeReader<T> : TypeReader
    where T : IConstrainedIntFactory
{
    private readonly int _minimumInclusive;
    private readonly int? _maximumInclusive;

    public ConstrainedIntTypeReader(int minimumInclusive, int? maximumInclusive = null)
    {
        _minimumInclusive = minimumInclusive;
        _maximumInclusive = maximumInclusive;
    }

    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        if (int.TryParse(input, out var parsed))
        {
            if (parsed < _minimumInclusive)
            {
                return Task.FromResult(TypeReaderResult.FromError(
                    CommandError.ParseFailed, $"Number '{input}' must be equal to or greater than {_minimumInclusive}."
                ));
            }

            if (_maximumInclusive.HasValue && parsed > _maximumInclusive.Value)
            {
                return Task.FromResult(TypeReaderResult.FromError(
                    CommandError.ParseFailed, $"Number '{input}' must be equal to or less than {_maximumInclusive}."
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
