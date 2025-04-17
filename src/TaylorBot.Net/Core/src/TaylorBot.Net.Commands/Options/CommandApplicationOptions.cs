﻿namespace TaylorBot.Net.Commands.Options;

public class CommandApplicationOptions
{
    public Dictionary<string, DailyLimit> DailyLimits { get; set; } = null!;
}

public class DailyLimit
{
    public string FriendlyName { get; set; } = null!;
    public uint MaxUsesForUser { get; set; }
    public uint? MaxUsesForPlusUser { get; set; }
}
