using Microsoft.Extensions.DependencyInjection;

namespace TaylorBot.Net.Commands.Preconditions;

public class InGuildPrecondition(CommandMentioner mention, bool botMustBeInGuild = false) : ICommandPrecondition
{
    public class Factory(IServiceProvider services)
    {
        public InGuildPrecondition Create(bool botMustBeInGuild = false) =>
            ActivatorUtilities.CreateInstance<InGuildPrecondition>(services, botMustBeInGuild);
    }

    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.Guild == null)
        {
            return new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} can only be used in a guild",
                UserReason: new(await mention.FormatAsync(context, $"You can't use {mention.Command(command)} because it can only be used in a server 🚫"))
            );
        }

        if (botMustBeInGuild && context.Guild.Fetched == null)
        {
            return new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} requires bot to be in guild",
                UserReason: new(
                    await mention.FormatAsync(context, $"""
                    You can't use {mention.Command(command)} because it requires TaylorBot to be added to this server 🥲
                    Ask a server admin to add it ✨ https://discord.com/oauth2/authorize?client_id=168767327024840704
                    """))
            );
        }

        return new PreconditionPassed();
    }
}
