using Npgsql;
using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Hosting;

internal static class ScenarioConfiguration
{
    public static Dictionary<string, string?> Create(DataServices services, NpgsqlConnectionStringBuilder connection, UserNotifierJob? job)
    {
        Dictionary<string, string?> settings = new()
        {
            ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = "no_application_insights",
            ["Discord:Token"] = "MTAwMDAwMDAwMDAwMDAwMDAx.synthetic.synthetic-test-token-never-a-real-credential",
            ["Discord:ShardCount"] = "1",
            ["Discord:StartupDelay"] = "00:00:00",
            ["DatabaseConnection:Host"] = connection.Host,
            ["DatabaseConnection:Port"] = $"{connection.Port}",
            ["DatabaseConnection:Username"] = connection.Username,
            ["DatabaseConnection:Password"] = connection.Password,
            ["DatabaseConnection:Database"] = connection.Database,
            ["DatabaseConnection:ApplicationName"] = "NotifierIntegrationTests",
            ["DatabaseConnection:MaxPoolSize"] = "10",
            ["DatabaseConnection:GssEncryptionMode"] = nameof(GssEncryptionMode.Disable),
            ["RedisConnection:Host"] = services.RedisHost,
            ["RedisConnection:Port"] = $"{services.RedisPort}",
            ["RedisConnection:Password"] = "",
            ["EntityTracker:UseRedisCache"] = "true",
            ["EntityTracker:TimeSpanBetweenGuildProcessedInReady"] = "00:00:00",
            ["MessageDeleted:UseRedisCache"] = "true",
            ["MemberLogging:FirstJoinedEmbedColor"] = "#74D600",
            ["MemberLogging:RejoinedEmbedColor"] = "#009C1A",
            ["MemberLeft:MemberLeftEmbedColorHex"] = "#CC0000",
            ["MemberBan:MemberBannedEmbedColorHex"] = "#CC0000",
            ["MemberBan:MemberUnbannedEmbedColorHex"] = "#00CC00",
            ["QuickStartEmbed:Title"] = "Getting started",
            ["QuickStartEmbed:Description"] = "Welcome to TaylorBot",
            ["QuickStartEmbed:Color"] = "#00C3FF",
            ["QuickStartEmbed:Fields:0:Name"] = "Help",
            ["QuickStartEmbed:Fields:0:Value"] = "Use /help",
            ["MinutesTracker:MinimumTimeSpanSinceLastSpoke"] = "00:05:00",
            ["MessagesTracker:TimeSpanBetweenPersistingTextChannelMessages"] = "00:01:00",
            ["MessagesTracker:TimeSpanBetweenPersistingMemberMessagesAndWords"] = "00:01:00",
            ["MessagesTracker:TimeSpanBetweenPersistingLastSpoke"] = "00:01:00",
            ["BirthdayRewardNotifier:RewardAmount"] = "1989",
            ["BirthdayRewardNotifier:TimeSpanBetweenRewards"] = "01:00:00",
            ["BirthdayRewardNotifier:TimeSpanBetweenMessages"] = "00:00:01",
            ["BirthdayRole:TimeSpanBetweenAdding"] = "04:00:00",
            ["BirthdayRole:TimeSpanBetweenRemoving"] = "01:00:00",
            ["ReminderNotifier:TimeSpanBetweenReminderChecks"] = "00:01:00",
            ["ReminderNotifier:TimeSpanBetweenMessages"] = "00:00:00",
            ["PatreonSync:Enabled"] = "false",
            ["PatreonSync:CampaignId"] = "13",
            ["PatreonSync:ApiKey"] = "synthetic",
            ["PatreonSync:TimeSpanBetweenSyncs"] = "01:00:00",
            ["PatreonSync:TimeSpanBetweenMessages"] = "00:00:00",
            ["RedditAuth:AppId"] = "synthetic",
            ["RedditAuth:AppSecret"] = "synthetic",
            ["YoutubeAuth:ApiKey"] = "synthetic",
            ["TumblrAuth:ConsumerKey"] = "synthetic",
            ["TumblrAuth:ConsumerSecret"] = "synthetic",
            ["TumblrAuth:Token"] = "synthetic",
            ["TumblrAuth:TokenSecret"] = "synthetic",
        };

        foreach (var color in new[] { "MessageReactionRemovedEmbedColorHex", "MessageDeletedEmbedColorHex", "MessageBulkDeletedEmbedColorHex", "MessageEditedEmbedColorHex" })
        {
            settings[$"MessageDeleted:{color}"] = "#00C3FF";
        }

        foreach (var service in new[] { "Reddit", "Youtube", "Tumblr" })
        {
            settings[$"{service}Notifier:TimeSpanBetweenRequests"] = "00:00:01";
            settings[$"{service}Notifier:{service}PostEmbedColor"] = "#00C3FF";
        }

        foreach (var name in new[] { "Reddit", "Youtube", "Tumblr", "BirthdayCalendar", "Reminder", "PatreonSync", "BirthdayRoleAdd", "BirthdayRoleRemove", "BirthdayReward" })
        {
            settings[$"UserNotifierStartup:{name}InitialDelay"] = "30.00:00:00";
        }

        var selected = job switch
        {
            UserNotifierJob.Reminders => "Reminder",
            UserNotifierJob.Patreon => "PatreonSync",
            UserNotifierJob.BirthdayRewards => "BirthdayReward",
            UserNotifierJob.Reddit or UserNotifierJob.Youtube or UserNotifierJob.Tumblr or UserNotifierJob.BirthdayCalendar
                or UserNotifierJob.BirthdayRoleAdd or UserNotifierJob.BirthdayRoleRemove => job.ToString(),
            _ => null,
        };
        if (selected != null)
        {
            settings[$"UserNotifierStartup:{selected}InitialDelay"] = "00:00:01";
        }

        return settings;
    }
}
