using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace TaylorBot.Net.Commands.DiscordNet;

public interface ITaylorBotCommandContext : ICommandContext
{
    string CommandPrefix { get; }
    IList<CommandInfo> CommandInfos { get; set; }
    RunContext? RunContext { get; set; }
    ISelfUser CurrentUser { get; }
    string GetUsage(CommandInfo commandInfo);
}

public class TaylorBotShardedCommandContext : ShardedCommandContext, ITaylorBotCommandContext
{
    public string CommandPrefix { get; }
    public IList<CommandInfo> CommandInfos { get; set; } = new List<CommandInfo>();
    public RunContext? RunContext { get; set; }
    public ISelfUser CurrentUser => Client.CurrentUser;

    public TaylorBotShardedCommandContext(DiscordShardedClient client, SocketUserMessage msg, string commandPrefix) : base(client, msg)
    {
        CommandPrefix = commandPrefix;
    }

    public string GetUsage(CommandInfo command)
    {
        return $"{CommandPrefix}{command.Aliases[0]} {string.Join(" ", command.Parameters.Select(p => $"<{p.Name}{(p.IsOptional ? "?" : "")}>"))}".TrimEnd();
    }
}
