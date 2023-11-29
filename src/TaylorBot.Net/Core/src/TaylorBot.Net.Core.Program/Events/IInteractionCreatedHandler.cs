using TaylorBot.Net.Core.Client;

namespace TaylorBot.Net.Core.Program.Events;

public interface IInteractionCreatedHandler
{
    Task InteractionCreatedAsync(Interaction interaction);
}
