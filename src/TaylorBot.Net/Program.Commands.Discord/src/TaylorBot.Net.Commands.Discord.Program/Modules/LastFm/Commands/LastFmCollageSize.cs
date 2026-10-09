using TaylorBot.Net.Commands.Types;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public class LastFmCollageSize(int parsed) : IConstrainedInt
{
    public int Parsed { get; } = parsed;
}
