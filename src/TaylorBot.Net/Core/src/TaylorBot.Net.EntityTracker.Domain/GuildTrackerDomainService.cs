using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.EntityTracker.Domain.Guild;
using TaylorBot.Net.EntityTracker.Domain.GuildName;

namespace TaylorBot.Net.EntityTracker.Domain;

public partial class GuildTrackerDomainService(ILogger<GuildTrackerDomainService> logger, IGuildRepository guildRepository, IGuildNameRepository guildNameRepository)
{
    public async ValueTask TrackGuildAndNameAsync(IGuild guild)
    {
        var guildAddedResult = await guildRepository.AddGuildIfNotAddedAsync(guild);

        await TrackGuildNameAsync(guild, guildAddedResult);
    }

    public async Task TrackGuildNameAsync(IGuild guild, GuildAddedResult guildAddedResult)
    {
        if (guildAddedResult.WasAdded)
        {
            LogAddedNewGuild(guild.FormatLog());
            await guildNameRepository.AddNewGuildNameAsync(guild);
        }
        else if (guildAddedResult.WasGuildNameChanged)
        {
            await guildNameRepository.AddNewGuildNameAsync(guild);
            LogAddedNewGuildName(
                guild.FormatLog(),
                guildAddedResult.PreviousGuildName != null ? $", previously was '{guildAddedResult.PreviousGuildName}'" : "");
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Added new guild {Guild}.")]
    private partial void LogAddedNewGuild(string guild);

    [LoggerMessage(Level = LogLevel.Information, Message = "Added new guild name for {Guild}{PreviousNameText}.")]
    private partial void LogAddedNewGuildName(string guild, string previousNameText);
}
