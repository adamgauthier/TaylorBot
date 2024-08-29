using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TaylorBot.Net.Commands.Instrumentation;

namespace TaylorBot.Net.Commands.DiscordNet;

public interface ITaylorBotCommandContext : ICommandContext
{
    Lazy<CommandActivity> Activity { get; }
    string CommandPrefix { get; }
    IList<CommandInfo> CommandInfos { get; set; }
    RunContext? RunContext { get; set; }
    ISelfUser CurrentUser { get; }
    string GetUsage(CommandInfo commandInfo);
}

public class TaylorBotShardedCommandContext(DiscordShardedClient client, SocketUserMessage msg, string commandPrefix, Lazy<CommandActivity> activity) : ShardedCommandContext(client, msg), ITaylorBotCommandContext
{
    public Lazy<CommandActivity> Activity { get; } = activity;
    public string CommandPrefix { get; } = commandPrefix;
    public IList<CommandInfo> CommandInfos { get; set; } = [];
    public RunContext? RunContext { get; set; }
    public ISelfUser CurrentUser => Client.CurrentUser;

    public string GetUsage(CommandInfo command)
    {
        return $"{CommandPrefix}{command.Aliases[0]} {string.Join(" ", command.Parameters.Select(p => $"<{p.Name}{(p.IsOptional ? "?" : "")}>"))}".TrimEnd();
    }
}
