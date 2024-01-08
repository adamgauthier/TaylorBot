using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;

public interface IModChannelLogger
{
    ValueTask<ITextChannel?> GetModLogAsync(IGuild guild);
    ValueTask<bool> TrySendModLogAsync(IGuild guild, IUser moderator, IUser user, Func<EmbedBuilder, EmbedBuilder> buildEmbed);
    Embed CreateResultEmbed(RunContext context, bool wasLogged, string successMessage);
}

public class ModChannelLogger(ILogger<ModChannelLogger> logger, IModLogChannelRepository modLogChannelRepository) : IModChannelLogger
{
    public async ValueTask<ITextChannel?> GetModLogAsync(IGuild guild)
    {
        var modLog = await modLogChannelRepository.GetModLogForGuildAsync(guild);
        if (modLog != null)
        {
            var channel = (ITextChannel?)await guild.GetChannelAsync(modLog.ChannelId.Id);
            return channel;
        }
        return null;
    }

    public async ValueTask<bool> TrySendModLogAsync(IGuild guild, IUser moderator, IUser user, Func<EmbedBuilder, EmbedBuilder> buildEmbed)
    {
        var channel = await GetModLogAsync(guild);

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
                logger.LogWarning(e, $"Error when sending mod log in {channel.FormatLog()}:");
            }
        }

        return false;
    }

    public Embed CreateResultEmbed(RunContext context, bool wasLogged, string successMessage)
    {
        return wasLogged ?
            EmbedFactory.CreateSuccess(successMessage) :
            EmbedFactory.CreateWarning(string.Join('\n', new[] {
                successMessage,
                "However, I was not able to log this action in your moderation log channel.",
                $"Make sure you set it up with {context.MentionCommand("mod log set")} and TaylorBot has access to it."
            }));
    }
}
