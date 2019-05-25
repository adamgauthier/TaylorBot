namespace TaylorBot.Net.EntityTracker.Domain.User
{
    public class UserAddedResult
    {
        public bool WasAdded { get; }

        public UserAddedResult(bool wasAdded)
        {
            WasAdded = wasAdded;
        }
    }
}
