namespace TaylorBot.Net.Commands.Preconditions;

public class TaylorBotOwnerPrecondition(CommandMentioner mention) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        var application = await context.Client.GetApplicationInfoAsync();

        if (context.User.Id == application.Owner.Id)
        {
            return new PreconditionPassed();
        }
        else
        {
            return new PreconditionFailed(
                PrivateReason: $"{command.Metadata.Name} can only be used by owner",
                UserReason: new($"You can't use {mention.Command(command, context)} because it can only be used by the bot owner 🚫")
            );
        }
    }
}
