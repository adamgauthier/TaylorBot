// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1849:Call async methods when in an async method", Justification = "SocketGuild sync methods mean from cache, async means from REST", Scope = "member", Target = "~M:TaylorBot.Net.BirthdayReward.Domain.BirthdayRoleDomainService.AddBirthdayRolesAsync~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Performance", "CA1849:Call async methods when in an async method", Justification = "SocketGuild sync methods mean from cache, async means from REST", Scope = "member", Target = "~M:TaylorBot.Net.BirthdayReward.Domain.BirthdayRoleDomainService.RemoveBirthdayRolesAsync~System.Threading.Tasks.Task")]
