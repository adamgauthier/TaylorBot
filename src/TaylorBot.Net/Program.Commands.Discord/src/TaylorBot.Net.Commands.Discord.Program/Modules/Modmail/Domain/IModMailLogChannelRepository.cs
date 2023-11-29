using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;

public interface IModMailLogChannelRepository
{
    ValueTask AddOrUpdateModMailLogAsync(ITextChannel textChannel);
    ValueTask RemoveModMailLogAsync(IGuild guild);
    ValueTask<ModLog?> GetModMailLogForGuildAsync(IGuild guild);
}
