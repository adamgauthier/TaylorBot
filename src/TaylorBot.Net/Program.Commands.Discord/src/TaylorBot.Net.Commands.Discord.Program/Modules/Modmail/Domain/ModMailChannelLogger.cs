﻿using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;

public class ModMailChannelLogger(ILogger<ModMailChannelLogger> logger, IModMailLogChannelRepository modMailLogChannelRepository, IModChannelLogger modChannelLogger)
{
    public async ValueTask<ITextChannel?> GetModMailLogAsync(IGuild guild)
    {
        var modLog = await modMailLogChannelRepository.GetModMailLogForGuildAsync(guild);
        if (modLog != null)
        {
            var channel = (ITextChannel?)await guild.GetChannelAsync(modLog.ChannelId.Id);
            return channel;
        }

        return await modChannelLogger.GetModLogAsync(guild);
    }

    public async ValueTask<bool> TrySendModMailLogAsync(IGuild guild, IUser moderator, IUser user, Func<EmbedBuilder, EmbedBuilder> buildEmbed)
    {
        var channel = await GetModMailLogAsync(guild);

        if (channel != null)
        {
            try
            {
                var baseEmbed = new EmbedBuilder()
                    .AddField("Moderator", moderator.FormatTagAndMention(), inline: true)
                    .AddField("User", user.FormatTagAndMention(), inline: true)
                    .WithCurrentTimestamp();

                await channel.SendMessageAsync(embed: buildEmbed(baseEmbed).Build());

                return true;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Error when sending mod mail log in {Channel}:", channel.FormatLog());
            }
        }

        return false;
    }

    public Embed CreateResultEmbed(RunContext context, bool wasLogged, string successMessage)
    {
        return modChannelLogger.CreateResultEmbed(context, wasLogged, successMessage);
    }
}
