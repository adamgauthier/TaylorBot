﻿using Discord;

namespace TaylorBot.Net.QuickStart.Domain;

public class QuickStartChannelFinder
{
    public async ValueTask<TChannel?> FindQuickStartChannelAsync<TGuild, TChannel>(TGuild guild)
        where TGuild : IGuild
        where TChannel : class, ITextChannel
    {
        var textChannels = (await guild.GetTextChannelsAsync()).Cast<TChannel>();
        var currentUser = await guild.GetCurrentUserAsync();

        var availableChannels = textChannels.Where(channel =>
           IsEveryoneAllowedToSendMessages(guild, channel) &&
           currentUser.GetPermissions(channel).Has(ChannelPermission.SendMessages)
        ).ToList();

        if (availableChannels.Count == 0)
            return null;

        foreach (var name in new[] { "general", "main" })
        {
            var namedGeneral = availableChannels.FirstOrDefault(channel => channel.Name.Contains(name, StringComparison.InvariantCulture));

            if (namedGeneral != null)
                return namedGeneral;
        }

        return availableChannels.First();
    }

    private static bool IsEveryoneAllowedToSendMessages(IGuild guild, ITextChannel channel)
    {
        var overwrite = channel.GetPermissionOverwrite(guild.EveryoneRole);
        return !overwrite.HasValue || overwrite.Value.SendMessages != PermValue.Deny;
    }
}
