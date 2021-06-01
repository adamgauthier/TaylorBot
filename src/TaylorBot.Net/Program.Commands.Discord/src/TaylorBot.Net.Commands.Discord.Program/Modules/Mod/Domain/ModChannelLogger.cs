using Discord;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain
{
    public interface IModChannelLogger
    {
        ValueTask<ITextChannel?> GetModLogAsync(IGuild guild);
        ValueTask TrySendModLogAsync(ITextChannel channel, IUser moderator, IUser user, Func<EmbedBuilder, EmbedBuilder> buildEmbed);
        ValueTask TrySendModLogAsync(IGuild guild, IUser moderator, IUser user, Func<EmbedBuilder, EmbedBuilder> buildEmbed);
    }

    public class ModChannelLogger : IModChannelLogger
    {
        private readonly ILogger<ModChannelLogger> _logger;
        private readonly IModLogChannelRepository _modLogChannelRepository;

        public ModChannelLogger(ILogger<ModChannelLogger> logger, IModLogChannelRepository modLogChannelRepository)
        {
            _logger = logger;
            _modLogChannelRepository = modLogChannelRepository;
        }

        public async ValueTask<ITextChannel?> GetModLogAsync(IGuild guild)
        {
            var modLog = await _modLogChannelRepository.GetModLogForGuildAsync(guild);
            if (modLog != null)
            {
                var channel = (ITextChannel?)await guild.GetChannelAsync(modLog.ChannelId.Id);
                return channel;
            }
            return null;
        }

        public async ValueTask TrySendModLogAsync(IGuild guild, IUser moderator, IUser user, Func<EmbedBuilder, EmbedBuilder> buildEmbed)
        {
            var channel = await GetModLogAsync(guild);

            if (channel != null)
            {
                await TrySendModLogAsync(channel, moderator, user, buildEmbed);
            }
        }

        public async ValueTask TrySendModLogAsync(ITextChannel channel, IUser moderator, IUser user, Func<EmbedBuilder, EmbedBuilder> buildEmbed)
        {
            try
            {
                var baseEmbed = new EmbedBuilder()
                    .AddField("Moderator", moderator.FormatTagAndMention(), inline: true)
                    .AddField("User", user.FormatTagAndMention(), inline: true)
                    .WithCurrentTimestamp();

                await channel.SendMessageAsync(embed: buildEmbed(baseEmbed).Build());
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Error when sending mod log in {channel.FormatLog()}:");
            }
        }
    }
}
