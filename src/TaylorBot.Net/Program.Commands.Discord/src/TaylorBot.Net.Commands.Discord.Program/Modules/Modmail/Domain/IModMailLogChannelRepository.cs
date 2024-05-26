using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;

public interface IModMailLogChannelRepository
{
    ValueTask AddOrUpdateModMailLogAsync(GuildTextChannel textChannel);
    ValueTask RemoveModMailLogAsync(IGuild guild);
    ValueTask<ModLog?> GetModMailLogForGuildAsync(IGuild guild);
}
