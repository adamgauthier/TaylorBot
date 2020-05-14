namespace TaylorBot.Net.EntityTracker.Domain.User
{
    public interface IUserAddedResult
    {
        bool WasAdded { get; }
        bool WasUsernameChanged { get; }
        string PreviousUsername { get; }
    }

    public class UserAddedResult : IUserAddedResult
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
