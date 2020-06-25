using Discord;
using System;

namespace TaylorBot.Net.Commands.StringMappers
{
    public class PermissionStringMapper
    {
        public string MapGuildPermissionToString(GuildPermission guildPermission)
        {
            return guildPermission switch
            {
                GuildPermission.ManageGuild => "Manage Server",
                GuildPermission.KickMembers => "Kick Members",
                GuildPermission.ManageRoles => "Manage Roles",
                _ => throw new ArgumentOutOfRangeException(nameof(guildPermission), guildPermission, "No mapping defined."),
            };
        }
    }
}
