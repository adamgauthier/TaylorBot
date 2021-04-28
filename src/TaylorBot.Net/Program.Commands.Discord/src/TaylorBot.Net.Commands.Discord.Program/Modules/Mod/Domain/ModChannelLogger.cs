using Discord;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain
{
    public interface IModChannelLogger
    {
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

        public async ValueTask TrySendModLogAsync(IGuild guild, IUser moderator, IUser user, Func<EmbedBuilder, EmbedBuilder> buildEmbed)
        {
            var modLog = await _modLogChannelRepository.GetModLogForGuildAsync(guild);
            if (modLog != null)
            {
                var channel = (ITextChannel?)await guild.GetChannelAsync(modLog.ChannelId.Id);
                if (channel != null)
                {
                    try
                    {
                        var baseEmbed = new EmbedBuilder()
                            .WithUserAsAuthor(user)
                            .AddField("Moderator", $"{moderator.Username}#{moderator.Discriminator} ({moderator.Mention})", inline: true)
                            .AddField("User", $"{user.Username}#{user.Discriminator} ({user.Mention})", inline: true)
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
    }
}
