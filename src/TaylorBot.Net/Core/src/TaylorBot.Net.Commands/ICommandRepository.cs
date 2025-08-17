namespace TaylorBot.Net.Commands;

public interface ICommandRepository
{
    public record Command(string Name);

    ValueTask<Command?> FindCommandByAliasAsync(string commandAlias);
}
