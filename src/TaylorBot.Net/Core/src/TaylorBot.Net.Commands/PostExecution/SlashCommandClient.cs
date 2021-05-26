using Discord;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Events;
using TaylorBot.Net.Core.Client;

namespace TaylorBot.Net.Commands.PostExecution
{
    public class SlashCommandClient
    {
        private const byte DeferredChannelMessageWithSourceInteractionResponseType = 5;

        private record InteractionResponse(byte type, InteractionResponse.InteractionApplicationCommandCallbackData? data)
        {
            public record InteractionApplicationCommandCallbackData(string? content = null, IReadOnlyList<Embed>? embeds = null, byte? flags = null);

            public record Embed(string? title, string? description, EmbedAuthor? author, EmbedImage? image, uint? color);

            public record EmbedAuthor(string? name, string? url, string? icon_url);

            public record EmbedImage(string? url);
        }

        private readonly Lazy<ITaylorBotClient> _taylorBotClient;
        private readonly HttpClient _httpClient;

        public SlashCommandClient(Lazy<ITaylorBotClient> taylorBotClient, HttpClient httpClient)
        {
            _taylorBotClient = taylorBotClient;
            _httpClient = httpClient;
        }

        public async ValueTask SendAcknowledgementResponseAsync(ApplicationCommand interaction)
        {
            var response = await _httpClient.PostAsync(
                $"https://discord.com/api/v8/interactions/{interaction.Id}/{interaction.Token}/callback",
                JsonContent.Create(new InteractionResponse(DeferredChannelMessageWithSourceInteractionResponseType, null))
            );

            response.EnsureSuccessStatusCode();
        }

        private static InteractionResponse.Embed ToInteractionEmbed(Embed embed)
        {
            return new InteractionResponse.Embed(
                title: embed.Title,
                description: embed.Description,
                author: embed.Author.HasValue ? new(embed.Author.Value.Name, embed.Author.Value.Url, embed.Author.Value.IconUrl) : null,
                image: embed.Image.HasValue ? new(embed.Image.Value.Url) : null,
                color: embed.Color.HasValue ? embed.Color.Value.RawValue : null
            );
        }

        public async ValueTask SendFollowupResponseAsync(ApplicationCommand interaction, Embed embed)
        {
            var applicationInfo = await _taylorBotClient.Value.DiscordShardedClient.GetApplicationInfoAsync();

            var response = await _httpClient.PostAsync(
                $"https://discord.com/api/v8/webhooks/{applicationInfo.Id}/{interaction.Token}",
                JsonContent.Create(new InteractionResponse.InteractionApplicationCommandCallbackData(content: string.Empty, embeds: new[] { ToInteractionEmbed(embed) }))
            );

            response.EnsureSuccessStatusCode();
        }
    }
}
