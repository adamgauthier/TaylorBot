using Discord;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.BirthdayReward.Domain.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.BirthdayReward.Domain;

public record BirthdayRole(string guild_id, string role_id);

public record BirthdayUser(string user_id, DateTime birthday_end);

public record BirthdayRoleToRemove(string guild_id, string user_id, string role_id, DateTime set_at);

public interface IBirthdayRoleRepository
{
    Task<List<BirthdayUser>> GetBirthdayUsersAsync();
    Task<List<BirthdayRole>> GetRolesAsync();
    Task<List<SnowflakeId>> GetGuildsForUserAsync(SnowflakeId userId, IReadOnlyList<BirthdayRole> roles);
    Task CreateRoleGivenAsync(BirthdayUser birthdayUser, BirthdayRole role, DateTimeOffset setAt);
    Task<List<BirthdayRoleToRemove>> GetRolesToRemoveAsync();
    Task SetRoleRemovedAtAsync(BirthdayRoleToRemove toRemove);
    Task<DateTime?> GetLastTimeRoleWasGivenAsync(BirthdayUser user, SnowflakeId guildId);
}

public partial class BirthdayRoleDomainService(
    ILogger<BirthdayRoleDomainService> logger,
    IOptionsMonitor<BirthdayRoleOptions> optionsMonitor,
    IBirthdayRoleRepository birthdayRepository,
    Lazy<ITaylorBotClient> taylorBotClient)
{
    public async Task StartAddingBirthdayRolesAsync()
    {
        await Task.Delay(TimeSpan.FromMinutes(1));

        while (true)
        {
            var options = optionsMonitor.CurrentValue;

            try
            {
                await AddBirthdayRolesAsync();
            }
            catch (Exception e)
            {
                LogUnhandledExceptionAddingBirthdayRoles(e);
                await Task.Delay(TimeSpan.FromSeconds(10));
                continue;
            }

            await Task.Delay(options.TimeSpanBetweenAdding!.Value);
        }
    }

    public async Task AddBirthdayRolesAsync()
    {
        var roles = await birthdayRepository.GetRolesAsync();

        if (roles.Count > 0)
        {
            LogFoundConfiguredBirthdayRoles(roles.Count, string.Join(", ", roles.Select(r => $"{r}")));

            var rolesByGuild = roles.ToDictionary(r => r.guild_id);

            foreach (var birthdayUser in await birthdayRepository.GetBirthdayUsersAsync())
            {
                LogFoundBirthdayUser(birthdayUser);

                foreach (var guildId in await birthdayRepository.GetGuildsForUserAsync(birthdayUser.user_id, roles))
                {
                    try
                    {
                        var birthdayRole = rolesByGuild[guildId];

                        var lastTimeRoleGiven = await birthdayRepository.GetLastTimeRoleWasGivenAsync(birthdayUser, guildId);
                        if (lastTimeRoleGiven is null || (DateTimeOffset.UtcNow - lastTimeRoleGiven.Value) >= TimeSpan.FromDays(360))
                        {
                            var guild = taylorBotClient.Value.ResolveRequiredGuild(guildId);

                            var member = await taylorBotClient.Value.ResolveGuildUserAsync(guild, birthdayUser.user_id);
                            if (member != null)
                            {
                                SnowflakeId roleId = new(birthdayRole.role_id);
                                var role = guild.GetRole(roleId);

                                if (role != null)
                                {
                                    if (!member.RoleIds.Contains(roleId))
                                    {
                                        await member.AddRoleAsync(role, new RequestOptions { AuditLogReason = "Given as a birthday role" });
                                        LogAddedBirthdayRoleTo(roleId, member.FormatLog());
                                    }
                                    else
                                    {
                                        LogMemberAlreadyHasRole(member.FormatLog(), roleId);
                                    }

                                    await birthdayRepository.CreateRoleGivenAsync(birthdayUser, birthdayRole, setAt: DateTimeOffset.UtcNow);
                                    LogAddedBirthdayRoleToRemove(roleId, member.FormatLog());
                                }
                                else
                                {
                                    LogBirthdayRoleDoesntExist(roleId, guild.FormatLog());
                                    rolesByGuild.Remove(guildId);
                                }
                            }
                            else
                            {
                                LogUserNotInGuild(birthdayUser, guild.FormatLog());
                            }

                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }
                        else
                        {
                            LogMemberGottenRoleInPastYear(birthdayUser.user_id, guildId, birthdayRole.role_id, $"{lastTimeRoleGiven:o}");
                        }
                    }
                    catch (Exception e)
                    {
                        LogExceptionAddingBirthdayRole(e, birthdayUser, guildId);
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                }
            }
        }
        else
        {
            LogNoBirthdayRolesConfigured();
        }
    }

    public async Task StartRemovingBirthdayRolesAsync()
    {
        await Task.Delay(TimeSpan.FromMinutes(1));

        while (true)
        {
            var options = optionsMonitor.CurrentValue;

            try
            {
                await RemoveBirthdayRolesAsync();
            }
            catch (Exception e)
            {
                LogUnhandledExceptionRemovingBirthdayRoles(e);
                await Task.Delay(TimeSpan.FromSeconds(10));
                continue;
            }

            await Task.Delay(options.TimeSpanBetweenRemoving!.Value);
        }
    }

    public async Task RemoveBirthdayRolesAsync()
    {
        foreach (var roleToRemove in await birthdayRepository.GetRolesToRemoveAsync())
        {
            try
            {
                var guild = taylorBotClient.Value.ResolveRequiredGuild(roleToRemove.guild_id);

                var member = await taylorBotClient.Value.ResolveGuildUserAsync(guild, roleToRemove.user_id);
                if (member != null)
                {
                    SnowflakeId roleId = new(roleToRemove.role_id);
                    var role = guild.GetRole(roleId);

                    if (role != null)
                    {
                        if (member.RoleIds.Contains(roleId))
                        {
                            var keptFor = DateTimeOffset.UtcNow - roleToRemove.set_at;
                            await member.RemoveRoleAsync(role, new RequestOptions { AuditLogReason = $"Removed birthday role after {keptFor.Humanize(maxUnit: TimeUnit.Hour)}" });
                            LogRemovedBirthdayRoleAfter(roleId, member.FormatLog(), keptFor);
                        }
                        else
                        {
                            LogMemberDoesntHaveRole(member.FormatLog(), roleId);
                        }
                    }
                    else
                    {
                        LogRemovalRoleDoesntExist(roleToRemove.role_id, guild.FormatLog());
                    }
                }
                else
                {
                    LogCouldNotFindUserInGuild(roleToRemove.user_id, guild.FormatLog());
                }

                await birthdayRepository.SetRoleRemovedAtAsync(roleToRemove);
            }
            catch (Exception e)
            {
                LogExceptionRemovingBirthdayRole(e, roleToRemove);
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Found {Count} configured birthday roles: {Roles}")]
    private partial void LogFoundConfiguredBirthdayRoles(int count, string roles);

    [LoggerMessage(Level = LogLevel.Information, Message = "Found birthday user {User}")]
    private partial void LogFoundBirthdayUser(BirthdayUser user);

    [LoggerMessage(Level = LogLevel.Information, Message = "Added birthday role {RoleId} to {Member}")]
    private partial void LogAddedBirthdayRoleTo(SnowflakeId roleId, string member);

    [LoggerMessage(Level = LogLevel.Information, Message = "Member {Member} already has birthday role {RoleId}")]
    private partial void LogMemberAlreadyHasRole(string member, SnowflakeId roleId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Added birthday role {RoleId} to remove from {Member}")]
    private partial void LogAddedBirthdayRoleToRemove(SnowflakeId roleId, string member);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Birthday role {RoleId} in {Guild} doesn't seem to exist")]
    private partial void LogBirthdayRoleDoesntExist(SnowflakeId roleId, string guild);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Birthday user {User} doesn't seem to be part of {Guild} anymore")]
    private partial void LogUserNotInGuild(BirthdayUser user, string guild);

    [LoggerMessage(Level = LogLevel.Information, Message = "Member {UserId} in {Guild} has gotten the birthday role {RoleId} in the past year ({LastGiven})")]
    private partial void LogMemberGottenRoleInPastYear(SnowflakeId userId, SnowflakeId guild, string roleId, string lastGiven);

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception occurred when adding birthday role for User {User} in Guild {GuildId}")]
    private partial void LogExceptionAddingBirthdayRole(Exception exception, BirthdayUser user, SnowflakeId guildId);

    [LoggerMessage(Level = LogLevel.Information, Message = "No birthday roles configured")]
    private partial void LogNoBirthdayRolesConfigured();

    [LoggerMessage(Level = LogLevel.Information, Message = "Removed birthday role {RoleId} from {Member} after {KeptFor}")]
    private partial void LogRemovedBirthdayRoleAfter(SnowflakeId roleId, string member, TimeSpan keptFor);

    [LoggerMessage(Level = LogLevel.Information, Message = "Member {Member} doesn't have birthday role {RoleId}")]
    private partial void LogMemberDoesntHaveRole(string member, SnowflakeId roleId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Birthday role {RoleId} in {Guild} doesn't seem to exist")]
    private partial void LogRemovalRoleDoesntExist(string roleId, string guild);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not find user {UserId} in {Guild}")]
    private partial void LogCouldNotFindUserInGuild(string userId, string guild);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in AddBirthdayRolesAsync.")]
    private partial void LogUnhandledExceptionAddingBirthdayRoles(Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in RemoveBirthdayRolesAsync.")]
    private partial void LogUnhandledExceptionRemovingBirthdayRoles(Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception occurred when removing birthday role {ToRemove}")]
    private partial void LogExceptionRemovingBirthdayRole(Exception exception, BirthdayRoleToRemove toRemove);
}
