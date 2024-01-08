namespace TaylorBot.Net.EntityTracker.Domain.User;

public interface IUserAddedResult
{
    bool WasAdded { get; }
    bool WasUsernameChanged { get; }
    string? PreviousUsername { get; }
}

public class UserAddedResult(bool wasAdded, bool wasUsernameChanged, string? previousUsername) : IUserAddedResult
{
    public bool WasAdded { get; } = wasAdded;
    public bool WasUsernameChanged { get; } = wasUsernameChanged;
    public string? PreviousUsername { get; } = previousUsername;
}
