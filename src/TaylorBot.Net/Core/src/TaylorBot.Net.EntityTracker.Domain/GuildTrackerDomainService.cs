using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.EntityTracker.Domain.Guild;
using TaylorBot.Net.EntityTracker.Domain.GuildName;

namespace TaylorBot.Net.EntityTracker.Domain;

public class GuildTrackerDomainService
{
    private readonly ILogger<GuildTrackerDomainService> _logger;
    private readonly IGuildRepository _guildRepository;
    private readonly IGuildNameRepository _guildNameRepository;

    public GuildTrackerDomainService(ILogger<GuildTrackerDomainService> logger, IGuildRepository guildRepository, IGuildNameRepository guildNameRepository)
    {
        _logger = logger;
        _guildRepository = guildRepository;
        _guildNameRepository = guildNameRepository;
    }

    public async ValueTask TrackGuildAndNameAsync(IGuild guild)
    {
        var guildAddedResult = await _guildRepository.AddGuildIfNotAddedAsync(guild);

        await TrackGuildNameAsync(guild, guildAddedResult);
    }

    public async Task TrackGuildNameAsync(IGuild guild, GuildAddedResult guildAddedResult)
    {
        if (guildAddedResult.WasAdded)
        {
            _logger.LogInformation("Added new guild {Guild}.", guild.FormatLog());
            await _guildNameRepository.AddNewGuildNameAsync(guild);
        }
        else if (guildAddedResult.WasGuildNameChanged)
        {
            await _guildNameRepository.AddNewGuildNameAsync(guild);
            _logger.LogInformation(
                "Added new guild name for {Guild}{PreviousNameText}.",
                guild.FormatLog(),
                guildAddedResult.PreviousGuildName != null ? $", previously was '{guildAddedResult.PreviousGuildName}'" : "");
        }
    }
}
