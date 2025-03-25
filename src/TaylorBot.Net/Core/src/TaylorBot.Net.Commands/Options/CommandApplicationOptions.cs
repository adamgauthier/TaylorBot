namespace TaylorBot.Net.Commands.Options;

public record CommandApplicationOptions(Dictionary<string, CommandApplicationOptions.DailyLimit> DailyLimits)
{
    public record DailyLimit(string FriendlyName, uint MaxUsesForUser, uint? MaxUsesForPlusUser);
}
