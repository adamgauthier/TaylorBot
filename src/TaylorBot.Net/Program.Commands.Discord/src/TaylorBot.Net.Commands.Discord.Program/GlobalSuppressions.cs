// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1849:Call async methods when in an async method", Justification = "SocketGuild sync methods mean from cache, async means from REST", Scope = "member", Target = "~M:TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands.BirthdayRoleSlashCommand.GetCommandAsync(TaylorBot.Net.Commands.RunContext,TaylorBot.Net.Commands.Parsers.NoOptions)~System.Threading.Tasks.ValueTask{TaylorBot.Net.Commands.Command}")]
[assembly: SuppressMessage("Performance", "CA1849:Call async methods when in an async method", Justification = "SocketGuild sync methods mean from cache, async means from REST", Scope = "member", Target = "~M:TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands.BirthdayRoleRemoveButtonHandler.HandleAsync(TaylorBot.Net.Commands.PostExecution.DiscordButtonComponent,TaylorBot.Net.Commands.RunContext)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Performance", "CA1849:Call async methods when in an async method", Justification = "SocketGuild sync methods mean from cache, async means from REST", Scope = "member", Target = "~M:TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Commands.JailModule.GetGuildJailRoleAsync~System.Threading.Tasks.ValueTask{TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Commands.JailModule.IGuildJailRoleResult}")]
[assembly: SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "Program file is fine", Scope = "type", Target = "~T:DiscordCommandsProgram")]
