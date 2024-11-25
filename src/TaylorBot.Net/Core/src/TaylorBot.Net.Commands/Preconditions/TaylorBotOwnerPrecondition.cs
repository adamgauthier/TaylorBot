namespace TaylorBot.Net.Commands.Preconditions;

public class TaylorBotOwnerPrecondition : ICommandPrecondition
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
                UserReason: new($"You can't use {context.MentionCommand(command)} because it can only be used by the bot owner 🚫")
            );
        }
    }
}
