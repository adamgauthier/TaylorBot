namespace TaylorBot.Net.MessagesTracker.Domain
{
    public class ChannelMessageCountChanged
    {
        public bool IsSpam { get; }

        public ChannelMessageCountChanged(bool isSpam)
        {
            IsSpam = isSpam;
        }
    }
}
