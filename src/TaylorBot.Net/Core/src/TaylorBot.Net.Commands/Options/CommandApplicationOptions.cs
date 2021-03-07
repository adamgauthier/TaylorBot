using System.Collections.Generic;

namespace TaylorBot.Net.Commands.Options
{
    public class CommandApplicationOptions
    {
        public string ApplicationName { get; set; } = null!;
        public Dictionary<string, uint> DailyLimits { get; set; } = null!;
    }
}
