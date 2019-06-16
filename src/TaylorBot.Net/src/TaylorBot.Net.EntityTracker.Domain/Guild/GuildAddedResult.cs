namespace TaylorBot.Net.EntityTracker.Domain.Guild
{
    public class GuildAddedResult
    {
        public bool WasAdded { get; }

        public GuildAddedResult(bool wasAdded)
        {
            WasAdded = wasAdded;
        }
    }
}
