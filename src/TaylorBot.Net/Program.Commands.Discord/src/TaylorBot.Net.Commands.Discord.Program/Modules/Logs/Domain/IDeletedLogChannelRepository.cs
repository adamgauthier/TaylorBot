﻿using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Domain
{
    public interface IDeletedLogChannelRepository
    {
        ValueTask AddOrUpdateDeletedLogAsync(ITextChannel textChannel);
        ValueTask RemoveDeletedLogAsync(IGuild guild);
    }
}