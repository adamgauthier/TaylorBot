﻿namespace TaylorBot.Net.Commands.Types;

public class PositiveInt32(int parsed) : IConstrainedInt
{
    public class Factory : IConstrainedIntFactory { public IConstrainedInt Create(int value) => new PositiveInt32(value); }

    public static int Min => 1;

    public int Parsed { get; } = parsed;
}
