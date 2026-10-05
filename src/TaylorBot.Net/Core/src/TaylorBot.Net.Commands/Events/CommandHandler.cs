using Discord.Commands;
using Discord.WebSocket;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Events;

public class CommandHandler(
    CommandActivityFactory commandActivityFactory,
    IServiceProvider serviceProvider,
    Lazy<ITaylorBotClient> taylorBotClient,
    CommandService commandService,
    CommandPrefixDomainService commandPrefixDomainService,
    CommandExecutedHandler commandExecutedHandler,
    BackgroundTasks backgroundTasks
    ) : IUserMessageReceivedHandler
{
    public async Task UserMessageReceivedAsync(SocketUserMessage userMessage)
    {
        if (userMessage.Author.IsBot)
            return;

        // Create a number to track where the prefix ends and the command begins
        var argPos = 0;

        var prefix = await commandPrefixDomainService.GetPrefixAsync(
            userMessage.Channel is SocketGuildChannel socketGuildChannel ? socketGuildChannel.Guild : null);

        if (!(userMessage.HasStringPrefix(prefix, ref argPos) ||
            userMessage.HasMentionPrefix(taylorBotClient.Value.DiscordShardedClient.CurrentUser, ref argPos)))
            return;

        TaylorBotShardedCommandContext context = new(
            taylorBotClient.Value.DiscordShardedClient, userMessage, prefix,
            new(() => commandActivityFactory.Create(CommandType.Prefix)));

        _ = backgroundTasks.Queue(() => ExecuteAsync(context, argPos), nameof(CommandHandler));
    }

    private async Task ExecuteAsync(ITaylorBotCommandContext context, int argPos)
    {
        try
        {
            IResult result;
            try
            {
                result = await commandService.ExecuteAsync(context, argPos, serviceProvider, MultiMatchHandling.Best);
            }
            catch (Exception exception)
            {
                result = ExecuteResult.FromError(exception);
            }

            await commandExecutedHandler.HandleResultAsync(context.MatchedCommand, context, result);
        }
        catch (Exception exception)
        {
            context.Activity.Value.SetError(exception);
            throw;
        }
        finally
        {
            await commandExecutedHandler.CleanupAsync(context);
        }
    }
}
