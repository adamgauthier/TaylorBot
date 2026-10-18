using Dapper;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Tracking;

public sealed class TrackingCheckpointMigrationTests(DataServices data)
{
    [Fact]
    public async Task UnusedCheckpoints_CanBeRevertedAndRedeployed()
    {
        await using var database = await data.CreateDatabaseAsync();
        await data.RunSqitchAsync(database, "revert", "--to-change", "20261012_user_ignore_state", "-y");
        await data.RunSqitchAsync(database, "deploy", "--verify");
        await data.RunSqitchAsync(database, "verify", "--from-change", "20261017_tracking_checkpoints");

        await using var connection = database.CreateConnection();
        var checkpoints = await connection.QueryAsync<long>("SELECT sequence FROM tracking.queue_checkpoints;");
        checkpoints.Should().HaveCount(3).And.OnlyContain(sequence => sequence == 0);
    }

    [Fact]
    public async Task UsedCheckpoints_CannotBeResetBySchemaRollback()
    {
        await using var database = await data.CreateDatabaseAsync();
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync("""
            UPDATE tracking.queue_checkpoints SET sequence = 1
            WHERE queue_name = 'member-message-count-increments';
            """);

        var revert = () => data.RunSqitchAsync(database, "revert", "--to-change", "20261012_user_ignore_state", "-y");
        await revert.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Cannot remove used tracking checkpoints*");

        (await connection.QuerySingleAsync<long>(
            "SELECT sequence FROM tracking.queue_checkpoints WHERE queue_name = 'member-message-count-increments';"))
            .Should().Be(1);
        await data.RunSqitchAsync(database, "verify", "--from-change", "20261017_tracking_checkpoints");
    }
}
