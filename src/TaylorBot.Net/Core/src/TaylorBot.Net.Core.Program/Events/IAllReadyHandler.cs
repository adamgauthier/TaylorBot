namespace TaylorBot.Net.Core.Program.Events;

public interface IAllReadyHandler
{
    Task AllShardsReadyAsync();
}
