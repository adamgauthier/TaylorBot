namespace TaylorBot.Net.EntityTracker.Domain.User
{
    public class UserAddedResult
    {
        public bool WasAdded { get; }
        public bool WasUsernameChanged { get; }
        public string PreviousUsername { get; }

        public UserAddedResult(bool wasAdded, bool wasUsernameChanged, string previousUsername)
        {
            WasAdded = wasAdded;
            WasUsernameChanged = wasUsernameChanged;
            PreviousUsername = previousUsername;
        }
    }
}
