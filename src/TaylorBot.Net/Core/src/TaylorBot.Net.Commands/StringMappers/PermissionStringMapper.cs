using Discord;

namespace TaylorBot.Net.Commands.StringMappers
{
    public class PermissionStringMapper
    {
        public string MapGuildPermissionToString(GuildPermission guildPermission)
        {
            return guildPermission switch
            {
                GuildPermission.ManageGuild => "Manage Server",
                GuildPermission.ManageChannels => "Manage Channels",
                GuildPermission.KickMembers => "Kick Members",
                GuildPermission.ModerateMembers => "Timeout Members",
                GuildPermission.ManageRoles => "Manage Roles",
                GuildPermission.BanMembers => "Ban Members",
                _ => throw new ArgumentOutOfRangeException(nameof(guildPermission), guildPermission, "No mapping defined."),
            };
        }
    }
}
