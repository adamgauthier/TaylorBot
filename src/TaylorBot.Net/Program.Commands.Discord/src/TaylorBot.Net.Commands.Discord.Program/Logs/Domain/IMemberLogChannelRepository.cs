﻿using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Logs.Domain
{
    public interface IMemberLogChannelRepository
    {
        ValueTask AddOrUpdateMemberLogAsync(ITextChannel textChannel);
        ValueTask RemoveMemberLogAsync(IGuild guild);
    }
}
