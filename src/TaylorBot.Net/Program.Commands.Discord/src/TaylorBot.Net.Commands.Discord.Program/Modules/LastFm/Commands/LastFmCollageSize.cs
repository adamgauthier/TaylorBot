using TaylorBot.Net.Commands.Types;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public class LastFmCollageSize : IConstrainedInt
{
    public class Factory : IConstrainedIntFactory { public IConstrainedInt Create(int value) => new LastFmCollageSize(value); }

    public static int Min => 3;
    public static int Max => 5;

    public int Parsed { get; }

    public LastFmCollageSize(int parsed)
    {
        Parsed = parsed;
    }
}

public class LastFmCollageSizeTypeReader : ConstrainedIntTypeReader<LastFmCollageSize.Factory>, ITaylorBotTypeReader
{
    public LastFmCollageSizeTypeReader() : base(LastFmCollageSize.Min, LastFmCollageSize.Max) { }

    public Type ArgumentType => typeof(LastFmCollageSize);
}
