// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = "OptionType is not meant to be called by child types", Scope = "member", Target = "~P:TaylorBot.Net.Commands.PostExecution.ISlashCommand`1.TaylorBot#Net#Commands#PostExecution#ISlashCommand#OptionType")]
[assembly: SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Needed to match command names", Scope = "member", Target = "~M:TaylorBot.Net.Commands.Types.CommandTypeReader.ReadAsync(Discord.Commands.ICommandContext,System.String,System.IServiceProvider)~System.Threading.Tasks.Task{Discord.Commands.TypeReaderResult}")]
[assembly: SuppressMessage("Performance", "CA1849:Call async methods when in an async method", Justification = "SocketGuild sync methods mean from cache, async means from REST", Scope = "member", Target = "~M:TaylorBot.Net.Commands.Types.CustomRoleTypeReader`1.ReadAsync(Discord.Commands.ICommandContext,System.String,System.IServiceProvider)~System.Threading.Tasks.Task{Discord.Commands.TypeReaderResult}")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed through MultipartFormDataContent", Scope = "member", Target = "~M:TaylorBot.Net.Commands.PostExecution.InteractionResponseClient.CreateContentWithAttachments(System.Collections.Generic.IReadOnlyList{TaylorBot.Net.Commands.Attachment},System.Net.Http.Json.JsonContent)~System.Net.Http.MultipartFormDataContent")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Removal planned soon", Scope = "member", Target = "~M:TaylorBot.Net.Commands.PostExecution.SlashCommandHandler.CreateAndBindButtons(TaylorBot.Net.Commands.PostExecution.ParsedInteraction,TaylorBot.Net.Commands.MessageResult,System.String)~System.Collections.Generic.List{TaylorBot.Net.Commands.Button}")]
