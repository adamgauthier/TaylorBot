using Discord;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;

public interface IGenderRepository
{
    ValueTask<string?> GetGenderAsync(IUser user);
    ValueTask SetGenderAsync(IUser user, string gender);
    ValueTask ClearGenderAsync(IUser user);
}
