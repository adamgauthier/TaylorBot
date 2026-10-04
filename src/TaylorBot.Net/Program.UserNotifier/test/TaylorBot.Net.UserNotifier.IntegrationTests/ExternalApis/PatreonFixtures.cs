using System.Text.Json;
using System.Text.Json.Nodes;
using TaylorBot.Net.IntegrationTests.Shared.ExternalApis;
using TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.ExternalApis;

public sealed record ScenarioPatron(ScenarioUser? User, bool Active = true, int EntitledCents = 200, bool Paid = false);

public sealed class PatreonFixtures(ExternalApi external)
{
    public const string MembersUri = "https://www.patreon.com/api/oauth2/v2/campaigns/13/members?include=user&fields[member]=email,full_name,last_charge_date,last_charge_status,lifetime_support_cents,currently_entitled_amount_cents,patron_status&fields[user]=social_connections";
    public const string ChargeDate = "2026-01-01T00:00:00.000Z";
    private const string NextUri = "https://www.patreon.com/api/oauth2/v2/campaigns/13/members?page[cursor]=next";

    public void Members(IReadOnlyList<ScenarioPatron> patrons, bool paginated = false)
    {
        if (paginated)
        {
            external.Json("GET", MembersUri, Page([patrons[0]], NextUri));
            external.Json("GET", NextUri, Page([.. patrons.Skip(1)]));
        }
        else
        {
            external.Json("GET", MembersUri, Page(patrons));
        }
    }

    private static JsonObject Page(IReadOnlyList<ScenarioPatron> patrons, string? next = null)
    {
        var page = JsonSerializer.SerializeToNode(new
        {
            data = patrons.Select((patron, index) => new
            {
                id = patron.User?.Id ?? $"unlinked-{index}",
                type = "member",
                attributes = new
                {
                    full_name = "Synthetic patron",
                    email = "patron@example.invalid",
                    last_charge_status = patron.Paid ? "Paid" : null,
                    last_charge_date = patron.Paid ? ChargeDate : null,
                    lifetime_support_cents = patron.EntitledCents,
                    currently_entitled_amount_cents = patron.EntitledCents,
                    patron_status = patron.Active ? "active_patron" : "former_patron",
                },
                relationships = new { user = new { data = new { id = patron.User?.Id ?? $"unlinked-{index}", type = "user" } } },
            }).ToArray(),
            included = patrons.Select((patron, index) => new
            {
                id = patron.User?.Id ?? $"unlinked-{index}",
                type = "user",
                attributes = new { social_connections = new { discord = patron.User == null ? null : new { user_id = patron.User.Id } } },
            }).ToArray(),
        })!.AsObject();

        if (next != null)
        {
            page["links"] = new JsonObject { ["next"] = next };
        }

        return page;
    }
}
