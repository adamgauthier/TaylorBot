namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

public sealed partial class ScenarioState
{
    public Task<int> CouponUsesAsync(string code = "ENCHANTED") =>
        ReadAsync<int>("SELECT used_count FROM commands.coupons WHERE code = @code;", new { code });

    public Task<long> CouponRewardAsync(string code) =>
        ReadAsync<long>("SELECT taypoint_reward FROM commands.coupons WHERE code = @code;", new { code });

    public Task<long> RedeemedCouponCountAsync(ScenarioUser user) =>
        ReadAsync<long>("SELECT count(*) FROM users.redeemed_coupons WHERE user_id = @Id;", new { user.Id });

    public Task<string?> LoveSenderAsync(ScenarioUser user) =>
        ReadAsync<string?>("SELECT acquired_from_user_id FROM valentines2026.role_obtained WHERE user_id = @Id;", new { user.Id });
}
