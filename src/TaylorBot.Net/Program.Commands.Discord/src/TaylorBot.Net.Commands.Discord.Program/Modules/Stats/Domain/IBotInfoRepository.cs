namespace TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Domain;

public interface IBotInfoRepository
{
    ValueTask<string> GetProductVersionAsync();
}
