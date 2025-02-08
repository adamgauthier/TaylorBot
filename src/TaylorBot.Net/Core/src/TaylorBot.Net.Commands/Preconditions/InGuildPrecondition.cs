using Microsoft.Extensions.DependencyInjection;

namespace TaylorBot.Net.Commands.Preconditions;

public class InGuildPrecondition(bool botMustBeInGuild = false) : ICommandPrecondition
{
    public class Factory(IServiceProvider services)
    {
        public InGuildPrecondition Create(bool botMustBeInGuild = false) =>
            ActivatorUtilities.CreateInstance<InGuildPrecondition>(services, botMustBeInGuild);
    }

    public ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.Guild == null)
        {
            return new(new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} can only be used in a guild",
                UserReason: new($"You can't use {context.MentionCommand(command)} because it can only be used in a server 🚫")
            ));
        }

        if (botMustBeInGuild && context.Guild.Fetched == null)
        {
            return new(new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} requires bot to be in guild",
                UserReason: new(
                    $"""
                    You can't use {context.MentionCommand(command)} because it requires TaylorBot to be added to this server 🥲
                    Ask a server admin to add it ✨ https://discord.com/oauth2/authorize?client_id=168767327024840704
                    """)
            ));
        }

        return new(new PreconditionPassed());
    }
}
