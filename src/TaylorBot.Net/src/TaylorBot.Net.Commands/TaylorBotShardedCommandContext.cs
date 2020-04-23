using Discord.Commands;
using Discord.WebSocket;
using System.Linq;

namespace TaylorBot.Net.Commands
{
    public interface ITaylorBotCommandContext : ICommandContext
    {
        string CommandPrefix { get; }
        string OnGoingCommandAddedToPool { get; set; }
        string GetUsage(CommandInfo commandInfo);
    }

    public class TaylorBotShardedCommandContext : ShardedCommandContext, ITaylorBotCommandContext
    {
        public string CommandPrefix { get; }
        public string OnGoingCommandAddedToPool { get; set; }

        public TaylorBotShardedCommandContext(DiscordShardedClient client, SocketUserMessage msg, string commandPrefix) : base(client, msg)
        {
            CommandPrefix = commandPrefix;
        }

        public string GetUsage(CommandInfo commandInfo)
        {
            return $"{CommandPrefix}{commandInfo.Name} {string.Join(" ", commandInfo.Parameters.Select(p => $"<{p.Name}{(p.IsOptional ? "?" : "")}>"))}";
        }
    }
}
