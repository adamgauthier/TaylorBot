namespace TaylorBot.Net.UserNotifier.Program.Options;

public record UserNotifierStartupOptions
{
    public TimeSpan RedditInitialDelay { get; set; }
    public TimeSpan YoutubeInitialDelay { get; set; }
    public TimeSpan TumblrInitialDelay { get; set; }
    public TimeSpan BirthdayCalendarInitialDelay { get; set; }
    public TimeSpan ReminderInitialDelay { get; set; }
    public TimeSpan PatreonSyncInitialDelay { get; set; }
    public TimeSpan BirthdayRoleAddInitialDelay { get; set; }
    public TimeSpan BirthdayRoleRemoveInitialDelay { get; set; }
    public TimeSpan BirthdayRewardInitialDelay { get; set; }
}
