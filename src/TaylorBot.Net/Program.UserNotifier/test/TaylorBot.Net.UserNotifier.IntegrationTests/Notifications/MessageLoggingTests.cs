namespace TaylorBot.Net.UserNotifier.IntegrationTests.Notifications;

public sealed class MessageLoggingTests(DataServices data)
{
    [Theory]
    [InlineData(0, false, "Message pinned")]
    [InlineData(100, false, "Message pinned")]
    [InlineData(0, true, "Message unpinned")]
    [InlineData(100, true, "Message unpinned")]
    public async Task PinStateChange_IdentifiesAction(int messageCacheSize, bool initiallyPinned, string expectedAction)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["Discord:MessageCacheSize"] = $"{messageCacheSize}" });
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, "Unchanged content", pinned: initiallyPinned);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.SetPinnedAsync(message, pinned: !initiallyPinned, partial: messageCacheSize > 0);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        var reversed = await scenario.Discord.SetPinnedAsync(message with { IsPinned = !initiallyPinned }, pinned: initiallyPinned);

        output.Messages.Single().Body!.Value.GetProperty("embeds")[0].GetProperty("footer").GetProperty("text")
            .GetString().Should().Be($"{expectedAction} ({message.Id})");
        reversed.Messages.Single().Body!.Value.GetProperty("embeds")[0].GetProperty("footer").GetProperty("text")
            .GetString().Should().Be($"Message {(initiallyPinned ? "pinned" : "unpinned")} ({message.Id})");
    }

    [Theory]
    [InlineData(0, true, 0)]
    [InlineData(100, true, 0)]
    [InlineData(0, false, 1)]
    [InlineData(100, false, 1)]
    [InlineData(0, false, 2)]
    [InlineData(100, false, 2)]
    public async Task EditedMessage_WithExistingState_RemainsAnEdit(int messageCacheSize, bool pinned, int flags)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["Discord:MessageCacheSize"] = $"{messageCacheSize}" });
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, "Original content", pinned: pinned, flags: flags);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.EditAsync(message, "Updated content");

        output.Messages.Single().Body!.Value.GetProperty("embeds")[0].GetProperty("footer").GetProperty("text")
            .GetString().Should().Be($"Message edited ({message.Id})");
        output.Text.Should().Contain("Original content");
    }

    [Fact]
    public async Task LegacyCacheWithoutMessageState_DoesNotGuessPublication()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["Discord:MessageCacheSize"] = "0" });
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, "Announcement");
        await scenario.Given.LegacyMessageCacheAsync(message);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.PublishAsync(message);

        output.Messages.Single().Body!.Value.GetProperty("embeds")[0].GetProperty("footer").GetProperty("text")
            .GetString().Should().Be($"Message edited ({message.Id})");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task PublishedMessage_IdentifiesAction(int messageCacheSize)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["Discord:MessageCacheSize"] = $"{messageCacheSize}" });
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, "Announcement");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.PublishAsync(message, partial: messageCacheSize > 0);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        var edited = await scenario.Discord.EditAsync(message with { Flags = 1 }, "Updated announcement");

        output.Messages.Single().Body!.Value.GetProperty("embeds")[0].GetProperty("footer").GetProperty("text")
            .GetString().Should().Be($"Message published ({message.Id})");
        edited.Messages.Single().Body!.Value.GetProperty("embeds")[0].GetProperty("footer").GetProperty("text")
            .GetString().Should().Be($"Message edited ({message.Id})");
        edited.Text.Should().Contain("Announcement");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task DeletedMessage_UsesAvailableCache(int messageCacheSize)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["Discord:MessageCacheSize"] = $"{messageCacheSize}" });
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "deleted");
        var message = await scenario.Discord.MessageAsync(guild, user, "A remembered message");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.DeleteAsync(message);

        output.Text.Should().Contain("A remembered message");
    }

    [Fact]
    public async Task EditedMessage_LogsBeforeAndAfterContent()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, "Original content");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.EditAsync(message, "Updated content");

        output.Text.Should().Contain("Original content");
        output.Messages.Single().Body!.Value.GetRawText().Should().Contain("Updated content");
    }

    [Fact]
    public async Task NoLogChannel_DoesNotSendDeletionLog()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var message = await scenario.Discord.MessageAsync(guild, user, "Unlogged");

        var output = await scenario.Discord.DeleteAsync(message);

        output.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task RemovedReaction_LogsMessageAndUser()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "deleted");
        var message = await scenario.Discord.MessageAsync(guild, user, "React here");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.RemoveReactionAsync(message, user);

        output.Messages.Single().Body!.Value.GetRawText().Should().Contain(message.Id).And.Contain(user.Username);
    }

    [Fact]
    public async Task BulkDeletion_BatchesCachedMessagesWithinDiscordEmbedLimit()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "deleted");
        var messages = await scenario.Discord.MessagesAsync(guild, user, count: 13);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.BulkDeleteAsync(guild, messages);

        output.Messages.Select(message => message.Body!.Value.GetProperty("embeds").GetArrayLength()).Should().Equal(10, 3);
        output.Text.Should().Contain("Message 12");
    }

    [Theory]
    [InlineData(0, 2000, 2)]
    [InlineData(100, 2000, 2)]
    [InlineData(0, 3000, 3)]
    [InlineData(100, 3000, 3)]
    public async Task BulkDeletion_LongMessagesRespectCombinedEmbedBudget(int messageCacheSize, int contentLength, int expectedBatches)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["Discord:MessageCacheSize"] = $"{messageCacheSize}" });
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "deleted");
        var messages = await scenario.Discord.MessagesAsync(guild, user, count: 3, content: new('a', contentLength));
        for (var index = 0; index < expectedBatches; index++)
        {
            scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        }

        var output = await scenario.Discord.BulkDeleteAsync(guild, messages);

        output.Messages.Should().HaveCount(expectedBatches);
        output.MessageEmbedTextLengths.Should().OnlyContain(length => length <= 6000);
        output.Messages.SelectMany(message => message.Body!.Value.GetProperty("embeds").EnumerateArray())
            .Select(embed => embed.GetProperty("description").GetString()).Should().Equal(messages.Select(message => message.Content));
    }

    [Theory]
    [InlineData(0, 6000, 1)]
    [InlineData(100, 6000, 1)]
    [InlineData(0, 6001, 2)]
    [InlineData(100, 6001, 2)]
    public async Task BulkDeletion_SplitsOnlyWhenCombinedBudgetIsExceeded(int messageCacheSize, int combinedLength, int expectedBatches)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["Discord:MessageCacheSize"] = $"{messageCacheSize}" });
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "deleted");
        var samples = await scenario.Discord.MessagesAsync(guild, user, count: 2, content: "a");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        var sampleOutput = await scenario.Discord.BulkDeleteAsync(guild, samples);
        var metadataLength = sampleOutput.MessageEmbedTextLengths.Single() - 2;
        var first = await scenario.Discord.MessageAsync(guild, user, new('a', count: 3000));
        var second = await scenario.Discord.MessageAsync(guild, user, new('b', combinedLength - metadataLength - first.Content.Length));
        for (var index = 0; index < expectedBatches; index++)
        {
            scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        }

        var output = await scenario.Discord.BulkDeleteAsync(guild, [first, second]);

        output.Messages.Should().HaveCount(expectedBatches);
        output.MessageEmbedTextLengths.Sum().Should().Be(combinedLength);
        output.MessageEmbedTextLengths.Should().OnlyContain(length => length <= 6000);
        output.Messages.SelectMany(message => message.Body!.Value.GetProperty("embeds").EnumerateArray())
            .Select(embed => embed.GetProperty("description").GetString()).Should().Equal(first.Content, second.Content);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(2, 3)]
    public async Task BulkDeletion_PreservesUncachedAndCachedMessages(int cachedCount, int expectedBatches)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["Discord:MessageCacheSize"] = "0" });
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var uncached = await scenario.Discord.MessagesAsync(guild, user, count: 98);
        await scenario.Given.LogChannelAsync(guild, "deleted");
        await scenario.Given.ExpireLogChannelCacheAsync(guild);
        var cached = await scenario.Discord.MessagesAsync(guild, user, cachedCount, content: new('a', count: 3000));
        for (var index = 0; index < expectedBatches; index++)
        {
            scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        }

        var output = await scenario.Discord.BulkDeleteAsync(guild, [.. uncached, .. cached]);

        output.Messages.Should().HaveCount(expectedBatches);
        output.MessageEmbedTextLengths.Should().OnlyContain(length => length <= 6000);
        output.Text.Should().ContainAll(uncached.Select(message => message.Id));
        output.Messages.SelectMany(message => message.Body!.Value.GetProperty("embeds").EnumerateArray())
            .Skip(3).Select(embed => embed.GetProperty("description").GetString()).Should().Equal(cached.Select(message => message.Content));
    }

    [Fact]
    public async Task DisabledPlusGuild_DoesNotSendConfiguredLogs()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "deleted", enabled: false);
        var message = await scenario.Discord.MessageAsync(guild, user, "Not entitled");

        var output = await scenario.Discord.DeleteAsync(message);

        output.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task UnknownDeletedMessage_StillLogsItsIdentity()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var message = await scenario.Discord.MessageAsync(guild, user, "Not cached");
        await scenario.Given.LogChannelAsync(guild, "deleted");
        await scenario.Given.ExpireLogChannelCacheAsync(guild);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.DeleteAsync(message);

        output.Messages.Single().Body!.Value.GetRawText().Should().Contain(message.Id).And.NotContain("Not cached");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task DeletedReply_PreservesAttachmentAndReferencedMessage(int messageCacheSize)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["Discord:MessageCacheSize"] = $"{messageCacheSize}" });
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "deleted");
        var original = await scenario.Discord.MessageAsync(guild, user, "Original");
        var reply = await scenario.Discord.MessageAsync(guild, user, "With a photo", type: 19, attachmentUrl: "https://images.invalid/photo.png", replyTo: original);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.DeleteAsync(reply);

        output.Messages.Single().Body!.Value.GetRawText().Should().Contain(original.Id).And.Contain("https://images.invalid/photo.png");
    }

    [Fact]
    public async Task BotEdit_UpdatesDeletionCacheWithoutSendingEditLog()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "deleted");
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, scenario.Given.Bot, "Bot message");

        var edit = await scenario.Discord.EditAsync(message, "Updated bot message");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        var deleted = await scenario.Discord.DeleteAsync(message);

        edit.Messages.Should().BeEmpty();
        deleted.Text.Should().Contain("Updated bot message");
    }

    [Fact]
    public async Task SystemMessage_IsLoggedWithoutIncrementingUserCounters()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "deleted");
        var message = await scenario.Discord.MessageAsync(guild, user, "", type: 6);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.DeleteAsync(message);
        await scenario.StopAsync();

        output.Text.Should().Contain("A message was pinned");
        (await scenario.State.MemberAsync(guild, user)).Messages.Should().Be(0);
    }

    [Fact]
    public async Task EmbedOnlyEdit_WithDiscordCache_DoesNotSendEditLog()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["Discord:MessageCacheSize"] = "100" });
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "edited");
        var message = await scenario.Discord.MessageAsync(guild, user, "https://example.invalid");

        var output = await scenario.Discord.RefreshEmbedsAsync(message);

        output.Messages.Should().BeEmpty();
    }
}
