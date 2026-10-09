using TaylorBot.Net.UserNotifier.IntegrationTests.Discord;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Notifications;

public sealed class MessageUpdateTests(DataServices data)
{
    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(100)]
    public async Task AutomaticPreviewUpdates_DoNotLogAndStillRefreshDeletionCache(int messageCacheSize)
    {
        await using var scenario = await CreateAsync(messageCacheSize);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        await scenario.Given.LogChannelAsync(guild, "deleted");
        var message = await scenario.Discord.MessageAsync(guild, user, "https://example.invalid");

        var first = await scenario.Discord.RefreshEmbedsAsync(message);
        var refreshed = await scenario.Discord.RefreshEmbedsAsync(message, description: "Refreshed preview");
        var duplicate = await scenario.Discord.RefreshEmbedsAsync(message, description: "Refreshed preview");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        var deleted = await scenario.Discord.DeleteAsync(message);

        first.Messages.Should().BeEmpty();
        refreshed.Messages.Should().BeEmpty();
        duplicate.Messages.Should().BeEmpty();
        deleted.Text.Should().Contain("https://example.invalid");
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(0, 3)]
    [InlineData(100, 0)]
    [InlineData(100, 1)]
    [InlineData(100, 3)]
    public async Task EmbedVisibilityChanges_LogSuppressionAndRestoration(int messageCacheSize, int embedCount)
    {
        await using var scenario = await CreateAsync(messageCacheSize);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, "https://example.invalid");
        await scenario.Discord.RefreshEmbedsAsync(message, count: embedCount);
        var preview = message with { Flags = 1024, PreviewDescription = "Link preview", PreviewCount = embedCount };
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var suppressed = await scenario.Discord.SuppressEmbedsAsync(preview, suppressed: true);
        var restored = await scenario.Discord.SuppressEmbedsAsync(preview with { Flags = 1028 }, suppressed: false);

        suppressed.Footer.Should().Be($"Message embeds suppressed ({message.Id})");
        restored.Footer.Should().Be($"Message embeds restored ({message.Id})");
        suppressed.Field("Embed Count").Should().Be($"{embedCount}");
        restored.Field("Embed Count").Should().Be($"{embedCount}");
        suppressed.Color.Should().Be(0x96B5DC);
        restored.Color.Should().Be(0xAEC8EB);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task CaptionChanges_DisplayEmptyBeforeAndAfterContent(int messageCacheSize)
    {
        await using var scenario = await CreateAsync(messageCacheSize);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, "Caption", attachmentUrl: "https://images.invalid/photo.png");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var removed = await scenario.Discord.EditAsync(message, "");
        var restored = await scenario.Discord.EditAsync(message with { Content = "" }, "Restored caption");

        removed.Text.Should().Contain("Caption");
        removed.Field("Message Content After Edit").Should().Contain("No text");
        restored.Text.Should().Contain("No text");
        restored.Field("Message Content After Edit").Should().Be("Restored caption");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task AttachmentChanges_ShowBeforeAndAfterWithoutMisreadingRenewedUrls(int messageCacheSize)
    {
        await using var scenario = await CreateAsync(messageCacheSize);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, "Caption", attachmentUrl: "https://images.invalid/photo.png?signature=old");

        var renewed = await scenario.Discord.UpdateAsync(message with { AttachmentUrl = "https://images.invalid/photo.png?signature=new" });
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        var removed = await scenario.Discord.UpdateAsync(message with { AttachmentUrl = null });
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        var added = await scenario.Discord.UpdateAsync(message with { AttachmentId = "100000000000070051" });

        renewed.Messages.Should().BeEmpty();
        removed.Footer.Should().Be($"Message attachments changed ({message.Id})");
        removed.Field("Attachments Before").Should().Contain("photo.png");
        removed.Field("Attachments Before").Should().Contain("signature=new");
        removed.Field("Attachments After").Should().Contain("None");
        added.Field("Attachments Before").Should().Contain("None");
        added.Field("Attachments After").Should().Contain("photo.png");
        removed.Color.Should().Be(0x91BDD9);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(100)]
    public async Task ThreadCreation_IsIdentifiedOnceWithoutMislabelingLaterEdits(int messageCacheSize)
    {
        await using var scenario = await CreateAsync(messageCacheSize);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, "Thread starter");
        await scenario.Discord.MessagesAsync(guild, user, count: 4);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var created = await scenario.Discord.CreateThreadAsync(message);
        var repeated = await scenario.Discord.CreateThreadAsync(message);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        var edited = await scenario.Discord.EditAsync(message with { Flags = 32 }, "Updated starter");

        created.Footer.Should().Be($"Thread create from message ({message.Id})");
        created.Field("Thread").Should().Contain(message.Id);
        created.Field("From").Should().Contain($"/channels/{guild.Id}/{guild.ChannelId}/{message.Id}");
        created.Color.Should().Be(0xA7B9E2);
        repeated.Messages.Should().BeEmpty();
        edited.Footer.Should().Be($"Message edited ({message.Id})");
        edited.Field("Link").Should().Contain($"/channels/{guild.Id}/{guild.ChannelId}/{message.Id}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task SimultaneousThreadCreationAndContentEdit_PreserveBothDetails(int messageCacheSize)
    {
        await using var scenario = await CreateAsync(messageCacheSize);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, "Original starter");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.CreateThreadAsync(message with { Content = "Updated starter" });

        output.Footer.Should().Be($"Thread create from message, message edited ({message.Id})");
        output.Field("From").Should().Contain($"/channels/{guild.Id}/{guild.ChannelId}/{message.Id}");
        output.Text.Should().Contain("Original starter");
        output.Field("Message Content After Edit").Should().Be("Updated starter");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task SimultaneousContentAndAttachmentChanges_PreserveBothDetails(int messageCacheSize)
    {
        await using var scenario = await CreateAsync(messageCacheSize);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, "Original", attachmentUrl: "https://images.invalid/photo.png");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.UpdateAsync(message with { Content = "Updated", AttachmentUrl = null });

        output.Footer.Should().Be($"Message edited ({message.Id})");
        output.Text.Should().Contain("Original");
        output.Field("Message Content After Edit").Should().Be("Updated");
        output.Field("Attachments Before").Should().Contain("photo.png");
        output.Field("Attachments After").Should().Contain("None");
    }

    [Fact]
    public async Task PublishedAnnouncementDeletion_UsesLastEditedContent()
    {
        await using var scenario = await CreateAsync(messageCacheSize: 0);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user, channels: [new("general", Type: 5)]);
        await scenario.Given.LogChannelAsync(guild, "edited");
        await scenario.Given.LogChannelAsync(guild, "deleted");
        var message = await scenario.Discord.MessageAsync(guild, user, "Announcement", channelType: 5);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var published = await scenario.Discord.PublishAsync(message);
        var edited = await scenario.Discord.EditAsync(message with { Flags = 1 }, "Updated announcement");
        var deleted = await scenario.Discord.DeleteAsync(message);

        published.Color.Should().Be(0x83AEE0);
        edited.Color.Should().Be(0x9BBFEA);
        deleted.Footer.Should().Be($"Message deleted ({message.Id})");
        deleted.Text.Should().Contain("Updated announcement");
    }

    [Fact]
    public async Task PinColors_AreDistinctFromContentEdits()
    {
        await using var scenario = await CreateAsync(messageCacheSize: 0);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, "Original");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var pinned = await scenario.Discord.SetPinnedAsync(message, pinned: true);
        var unpinned = await scenario.Discord.SetPinnedAsync(message with { IsPinned = true }, pinned: false);

        pinned.Color.Should().Be(0xAAC9EF);
        unpinned.Color.Should().Be(0xBDD2EE);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task DeletedPreview_LogsEmbedCountFromEitherCache(int messageCacheSize)
    {
        await using var scenario = await CreateAsync(messageCacheSize);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "deleted");
        var message = await scenario.Discord.MessageAsync(guild, user, "https://example.invalid");
        await scenario.Discord.RefreshEmbedsAsync(message);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.DeleteAsync(message);

        output.Field("Embed Count").Should().Be("1");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task AttachmentReplacement_WithUnchangedCount_IsIdentified(int messageCacheSize)
    {
        await using var scenario = await CreateAsync(messageCacheSize);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, "Caption", attachmentUrl: "https://images.invalid/before.png");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.UpdateAsync(message with { AttachmentId = "100000000000070051", AttachmentUrl = "https://images.invalid/after.png" });

        output.Footer.Should().Be($"Message attachments changed ({message.Id})");
        output.Field("Attachments Before").Should().Contain("before.png");
        output.Field("Attachments After").Should().Contain("after.png");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task DistributedAnnouncement_SourceDeletion_IsIdentifiedOnce(int messageCacheSize)
    {
        await using var scenario = await CreateAsync(messageCacheSize);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, "Distributed announcement", flags: 2);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var deleted = await scenario.Discord.UpdateAsync(message with { Flags = 10 });
        var repeated = await scenario.Discord.UpdateAsync(message with { Flags = 10 });

        deleted.Footer.Should().Be($"Message published source deleted ({message.Id})");
        repeated.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task UnknownPreviousState_RemainsExplicitAndStartsCachingSubsequentUpdates()
    {
        await using var scenario = await CreateAsync(messageCacheSize: 0);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var message = await scenario.Discord.MessageAsync(guild, user, "Unknown original");
        await scenario.Given.LogChannelAsync(guild, "edited");
        await scenario.Given.ExpireLogChannelCacheAsync(guild);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var edited = await scenario.Discord.EditAsync(message, "Known update");
        var preview = await scenario.Discord.RefreshEmbedsAsync(message with { Content = "Known update" });

        edited.Embed.GetProperty("title").GetString().Should().Contain("Unknown");
        edited.Field("Message Content After Edit").Should().Be("Known update");
        preview.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task CombinedLongContentAndAttachmentEdit_FitsDiscordEmbedBudget()
    {
        await using var scenario = await CreateAsync(messageCacheSize: 0);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, new('a', count: 4000),
            attachmentUrl: $"https://images.invalid/{new string('b', count: 1100)}.png");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.UpdateAsync(message with { Content = new('c', count: 2000), AttachmentUrl = null });

        output.EmbedTextLength.Should().BeLessThanOrEqualTo(6000);
        output.Field("Message Content After Edit").Should().StartWith("ccc");
        output.Field("Attachments Before").Should().Contain("photo");
        output.Field("Attachments After").Should().Contain("None");
    }

    private Task<UserNotifierScenario> CreateAsync(int messageCacheSize) =>
        UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, settings: new Dictionary<string, string?>
        {
            ["Discord:MessageCacheSize"] = $"{messageCacheSize}",
            ["MessageDeleted:MessageEditedEmbedColorHex"] = "#9BBFEA",
            ["MessageDeleted:MessagePinnedEmbedColorHex"] = "#AAC9EF",
            ["MessageDeleted:MessageUnpinnedEmbedColorHex"] = "#BDD2EE",
            ["MessageDeleted:MessagePublishedEmbedColorHex"] = "#83AEE0",
            ["MessageDeleted:MessageEmbedsSuppressedEmbedColorHex"] = "#96B5DC",
            ["MessageDeleted:MessageEmbedsRestoredEmbedColorHex"] = "#AEC8EB",
            ["MessageDeleted:MessageAttachmentsChangedEmbedColorHex"] = "#91BDD9",
            ["MessageDeleted:MessageThreadCreatedEmbedColorHex"] = "#A7B9E2",
        });
}
