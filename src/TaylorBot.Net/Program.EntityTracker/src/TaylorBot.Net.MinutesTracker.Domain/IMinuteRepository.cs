﻿using System;
using System.Threading.Tasks;

namespace TaylorBot.Net.MinutesTracker.Domain
{
    public interface IMinuteRepository
    {
        ValueTask AddMinutesToActiveMembersAsync(long minutesToAdd, TimeSpan minimumTimeSpanSinceLastSpoke, long minutesRequiredForReward, long pointsReward);
    }
}