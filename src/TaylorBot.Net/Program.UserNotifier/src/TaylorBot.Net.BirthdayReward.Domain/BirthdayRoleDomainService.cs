using Discord;
using Humanizer;
using Humanizer.Localisation;
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

public class BirthdayRoleDomainService(
    ILogger<BirthdayRoleDomainService> logger,
    IOptionsMonitor<BirthdayRoleOptions> optionsMonitor,
    IBirthdayRoleRepository birthdayRepository,
    Lazy<ITaylorBotClient> taylorBotClient)
{
    public async Task StartAddingBirthdayRolesAsync()
    {
        while (true)
        {
            var options = optionsMonitor.CurrentValue;

            try
            {
                await AddBirthdayRolesAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unhandled exception in {nameof(AddBirthdayRolesAsync)}.");
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
            var rolesByGuild = roles.ToDictionary(r => r.guild_id);

            foreach (var birthdayUser in await birthdayRepository.GetBirthdayUsersAsync())
            {
                foreach (var guildId in await birthdayRepository.GetGuildsForUserAsync(birthdayUser.user_id, roles))
                {
                    try
                    {
                        var birthdayRole = rolesByGuild[guildId];

                        var lastTimeRoleGiven = await birthdayRepository.GetLastTimeRoleWasGivenAsync(birthdayUser, guildId);
                        if (lastTimeRoleGiven is null || (DateTimeOffset.UtcNow - lastTimeRoleGiven.Value) >= TimeSpan.FromDays(360))
                        {
                            var guild = taylorBotClient.Value.ResolveRequiredGuild(guildId);

                            var member = await taylorBotClient.Value.ResolveGuildUserAsync(guild, birthdayUser.user_id)
                                ?? throw new ArgumentException($"Could not resolve User ID {birthdayUser.user_id}.");

                            SnowflakeId roleId = new(birthdayRole.role_id);
                            var role = guild.GetRole(roleId);

                            if (role != null)
                            {
                                if (!member.RoleIds.Contains(roleId))
                                {
                                    await member.AddRoleAsync(role, new RequestOptions { AuditLogReason = "Given as a birthday role" });
                                    logger.LogInformation("Added birthday role {RoleId} to {Member}", roleId, member.FormatLog());
                                }
                                else
                                {
                                    logger.LogInformation("Member {Member} already has birthday role {RoleId}", member.FormatLog(), roleId);
                                }

                                await birthdayRepository.CreateRoleGivenAsync(birthdayUser, birthdayRole, setAt: DateTimeOffset.UtcNow);
                                logger.LogInformation("Added birthday role {RoleId} to remove from {Member}", roleId, member.FormatLog());
                            }
                            else
                            {
                                logger.LogWarning("Birthday role {RoleId} in {Guild} doesn't seem to exist", roleId, guild.FormatLog());
                                rolesByGuild.Remove(guildId);
                            }
                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }
                        else
                        {
                            logger.LogInformation("Member {UserId} in {Guild} has gotten the birthday role {RoleId} in the past year ({LastGiven})", birthdayUser.user_id, guildId, birthdayRole.role_id, $"{lastTimeRoleGiven:o}");
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Exception occurred when adding birthday role for User {UserId} in Guild {GuildId}", birthdayUser, guildId);
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                }
            }
        }
        else
        {
            logger.LogInformation("No birthday roles configured");
        }
    }

    public async Task StartRemovingBirthdayRolesAsync()
    {
        while (true)
        {
            var options = optionsMonitor.CurrentValue;

            try
            {
                await RemoveBirthdayRolesAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unhandled exception in {nameof(RemoveBirthdayRolesAsync)}.");
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

                var member = await taylorBotClient.Value.ResolveGuildUserAsync(guild, roleToRemove.user_id)
                    ?? throw new ArgumentException($"Could not resolve User ID {roleToRemove.user_id}.");

                SnowflakeId roleId = new(roleToRemove.role_id);
                var role = guild.GetRole(roleId);

                if (role != null)
                {
                    if (member.RoleIds.Contains(roleId))
                    {
                        var keptFor = DateTimeOffset.UtcNow - roleToRemove.set_at;
                        await member.RemoveRoleAsync(role, new RequestOptions { AuditLogReason = $"Removed birthday role after {keptFor.Humanize(maxUnit: TimeUnit.Hour)}" });
                        logger.LogInformation("Removed birthday role {RoleId} from {Member} after {KeptFor}", roleId, member.FormatLog(), keptFor);
                    }
                    else
                    {
                        logger.LogInformation("Member {Member} doesn't have birthday role {RoleId}", member.FormatLog(), roleId);
                    }
                }
                else
                {
                    logger.LogWarning("Birthday role {RoleId} in {Guild} doesn't seem to exist", roleToRemove.role_id, guild.FormatLog());
                }

                await birthdayRepository.SetRoleRemovedAtAsync(roleToRemove);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception occurred when removing birthday role {ToRemove}", roleToRemove);
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
