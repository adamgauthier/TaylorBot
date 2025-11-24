using Discord;
using Microsoft.Extensions.Logging;
using OperationResult;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.User;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;

public partial class ModMailChannelLogger(ILogger<ModMailChannelLogger> logger, IModMailLogChannelRepository modMailLogChannelRepository, CommandMentioner mention)
{
    public async ValueTask<Result<ITextChannel, Embed>> GetModMailLogAsync(IGuild guild, RunContext context)
    {
        var modLog = await modMailLogChannelRepository.GetModMailLogForGuildAsync(new(guild.Id, guild));
        if (modLog == null)
        {
            return Error(CreateNotConfiguredModMailLogEmbed(context));
        }

        var channel = (ITextChannel?)await guild.GetChannelAsync(modLog.ChannelId);
        if (channel == null)
        {
            return Error(CreateChannelNotFoundModMailLogEmbed(context));
        }

        return Ok(channel);
    }

    public async ValueTask<bool> TrySendModMailLogAsync(RunContext context, IGuild guild, DiscordUser moderator, DiscordUser user, Func<EmbedBuilder, EmbedBuilder> buildEmbed, SnowflakeId? replyToMessageId = null)
    {
        var result = await GetModMailLogAsync(guild, context);
        if (result.IsSuccess)
        {
            try
            {
                var baseEmbed = new EmbedBuilder()
                    .AddField("Moderator", moderator.FormatTagAndMention(), inline: true)
                    .AddField("User", user.FormatTagAndMention(), inline: true)
                    .WithCurrentTimestamp();

                await result.Value.SendMessageAsync(embed: buildEmbed(baseEmbed).Build(), messageReference: replyToMessageId != null ? new(replyToMessageId) : null);

                return true;
            }
            catch (Exception e)
            {
                LogErrorSendingModMailLog(e, result.Value.FormatLog());
            }
        }

        return false;
    }

    public Embed CreateResultEmbed(RunContext context, bool wasLogged, string successMessage)
    {
        return wasLogged ?
            EmbedFactory.CreateSuccess(successMessage) :
            EmbedFactory.CreateWarning(
                $"""
                {successMessage}
                However, I was not able to log this action in your moderation log channel 😕
                Make sure you set it up with {mention.SlashCommand("modmail log-set", context)} and TaylorBot has access to it 🛠️
                """);
    }

    public Embed CreateNotConfiguredModMailLogEmbed(RunContext context)
    {
        return EmbedFactory.CreateError(
            $"""
            Sorry, this server hasn't enabled TaylorBot Mod Mail 😕
            Ask a moderator to set it up with {mention.SlashCommand("modmail log-set", context)} 🛠️
            """);
    }

    public Embed CreateChannelNotFoundModMailLogEmbed(RunContext context)
    {
        return EmbedFactory.CreateError(
            $"""
            Sorry, this server's TaylorBot Mod Mail channel no longer exists or TaylorBot can't access it 😕
            Ask a moderator to fix it with {mention.SlashCommand("modmail log-set", context)} and make sure TaylorBot has the right permissions 🛠️
            """);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Error when sending mod mail log in {Channel}:")]
    private partial void LogErrorSendingModMailLog(Exception exception, string channel);
}
