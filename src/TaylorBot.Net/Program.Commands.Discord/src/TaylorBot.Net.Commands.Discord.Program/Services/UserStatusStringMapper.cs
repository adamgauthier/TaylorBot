using Discord;

namespace TaylorBot.Net.Commands.Discord.Program.Services
{
    public class UserStatusStringMapper
    {
        public string MapStatusToString(UserStatus userStatus)
        {
            switch (userStatus)
            {
                case UserStatus.Online:
                    return "Online";
                case UserStatus.AFK:
                case UserStatus.Idle:
                    return "Idle";
                case UserStatus.DoNotDisturb:
                    return "Do Not Disturb";
                case UserStatus.Offline:
                    return "Offline";
                case UserStatus.Invisible:
                    return "Invisible";
                default:
                    throw new ArgumentOutOfRangeException(nameof(userStatus), userStatus, "No mapping defined.");
            }
        }
    }
}
