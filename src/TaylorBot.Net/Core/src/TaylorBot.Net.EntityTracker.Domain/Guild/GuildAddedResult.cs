namespace TaylorBot.Net.EntityTracker.Domain.Guild
{
    public class GuildAddedResult
    {
        public bool WasAdded { get; }
        public bool WasGuildNameChanged { get; }
        public string PreviousGuildName { get; }

        public GuildAddedResult(bool wasAdded, bool wasGuildNameChanged, string previousGuildName)
        {
            WasAdded = wasAdded;
            WasGuildNameChanged = wasGuildNameChanged;
            PreviousGuildName = previousGuildName;
        }
    }
}
