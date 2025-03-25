using System.Text.Json;
using TaylorBot.Net.Core.Infrastructure.Extensions;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.PatreonSync.Domain;

namespace TaylorBot.Net.PatreonSync.Infrastructure;

public class PatreonHttpClient(HttpClient httpClient) : IPatreonClient
{
    private static readonly string GetPatronsQueryString = new Dictionary<string, string>() {
        { "include", "user" },
        { "fields[member]", "email,full_name,last_charge_date,last_charge_status,lifetime_support_cents,currently_entitled_amount_cents,patron_status" },
        { "fields[user]", "social_connections" },
    }.ToUrlQueryString();

    public async ValueTask<IReadOnlyCollection<Patron>> GetPatronsWithDiscordAccountAsync(uint campaignId)
    {
        var results = await FetchPatronsAsync($"https://www.patreon.com/api/oauth2/v2/campaigns/{campaignId}/members?{GetPatronsQueryString}", MembersResult.Default);

        var joined = results.data.Join(
            results.included,
            data => data.relationships.user.data.id,
            included => included.id,
            (data, included) => (Data: data, Included: included)
        );

        return [.. joined
            .Where(member => member.Included.attributes.social_connections?.discord?.user_id != null)
            .Select(member =>
            {
                var discordUserId = member.Included.attributes.social_connections!.discord!.user_id;
                var lastCharge = member.Data.attributes.last_charge_status != null && member.Data.attributes.last_charge_date != null ?
                    new PatronLastCharge(
                        member.Data.attributes.last_charge_date,
                        member.Data.attributes.last_charge_status == "Paid" ? PatreonChargeStatus.Paid : PatreonChargeStatus.Other
                    ) :
                    null;

                return new Patron(
                    DiscordUserId: new SnowflakeId(discordUserId),
                    IsActive: member.Data.attributes.patron_status == "active_patron",
                    LastCharge: lastCharge,
                    CurrentlyEntitledAmountCents: member.Data.attributes.currently_entitled_amount_cents,
                    Metadata: member.ToString()
                );
            })];
    }

    private sealed record MembersResult(IReadOnlyCollection<CampaignMembersApi.Data> data, IReadOnlyCollection<CampaignMembersApi.Included> included)
    {
        public static readonly MembersResult Default = new([], []);
    }

    private sealed record CampaignMembersApi(
        IReadOnlyCollection<CampaignMembersApi.Data> data,
        IReadOnlyCollection<CampaignMembersApi.Included> included,
        CampaignMembersApi.Links? links
    )
    {
        public sealed record Data(DataAttributes attributes, Relationships relationships);
        public sealed record DataAttributes(
            string full_name, string email, string? last_charge_status, string? last_charge_date,
            int currently_entitled_amount_cents, int lifetime_support_cents, string? patron_status
        );
        public sealed record Relationships(UserRelationship user);
        public sealed record UserRelationship(UserRelationshipData data);
        public sealed record UserRelationshipData(string id);
        public sealed record Included(string id, IncludedAttributes attributes);
        public sealed record IncludedAttributes(Socials? social_connections);
        public sealed record Socials(SocialDiscord? discord);
        public sealed record SocialDiscord(string user_id);
        public sealed record Links(string next);
    }

    private async ValueTask<MembersResult> FetchPatronsAsync(string url, MembersResult result)
    {
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var campaignMembers = JsonSerializer.Deserialize<CampaignMembersApi>(json)!;

        var newResult = result with
        {
            data = [.. result.data, .. campaignMembers.data],
            included = [.. result.included, .. campaignMembers.included]
        };

        if (campaignMembers.links != null)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            return await FetchPatronsAsync(campaignMembers.links.next, newResult);
        }
        else
        {
            return newResult;
        }
    }
}
