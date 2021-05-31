using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;

namespace TaylorBot.Net.Commands.PostExecution
{
    public enum ButtonStyle { Primary, Secondary, Success, Danger }

    public record Button(string Id, ButtonStyle Style, string Label);

    public class InteractionResponseClient
    {
        private const byte DeferredChannelMessageWithSourceInteractionResponseType = 5;
        private const byte DeferredUpdateMessageInteractionResponseType = 6;

        private record InteractionResponse(byte type, InteractionResponse.InteractionApplicationCommandCallbackData? data)
        {
            public record InteractionApplicationCommandCallbackData(string? content = null, IReadOnlyList<Embed>? embeds = null, byte? flags = null, IReadOnlyList<Component>? components = null);

            public record Component(byte type, byte? style = null, string? label = null, string? custom_id = null, string? url = null, bool? disabled = null, IReadOnlyList<Component>? components = null);

            public record Embed(string? title, string? description, EmbedAuthor? author, EmbedImage? image, uint? color);

            public record EmbedAuthor(string? name, string? url, string? icon_url);

            public record EmbedImage(string? url);
        }

        private enum InteractionButtonStyle { Primary = 1, Secondary = 2, Success = 3, Danger = 4, Link = 5 }

        private readonly Lazy<ITaylorBotClient> _taylorBotClient;
        private readonly HttpClient _httpClient;

        public InteractionResponseClient(Lazy<ITaylorBotClient> taylorBotClient, HttpClient httpClient)
        {
            _taylorBotClient = taylorBotClient;
            _httpClient = httpClient;
        }

        public async ValueTask SendAcknowledgementResponseAsync(ApplicationCommand interaction, bool IsEphemeral = false)
        {
            var response = await _httpClient.PostAsync(
                $"https://discord.com/api/v8/interactions/{interaction.Id}/{interaction.Token}/callback",
                JsonContent.Create(new InteractionResponse(DeferredChannelMessageWithSourceInteractionResponseType, IsEphemeral ? new(flags: 64) : null))
            );

            response.EnsureSuccessStatusCode();
        }

        public async ValueTask SendComponentAcknowledgementResponseAsync(Interaction interaction)
        {
            var response = await _httpClient.PostAsync(
                $"https://discord.com/api/v8/interactions/{interaction.id}/{interaction.token}/callback",
                JsonContent.Create(new InteractionResponse(DeferredUpdateMessageInteractionResponseType, null))
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

        private static InteractionButtonStyle ToInteractionStyle(ButtonStyle style)
        {
            return style switch
            {
                ButtonStyle.Primary => InteractionButtonStyle.Primary,
                ButtonStyle.Secondary => InteractionButtonStyle.Secondary,
                ButtonStyle.Success => InteractionButtonStyle.Success,
                ButtonStyle.Danger => InteractionButtonStyle.Danger,
                _ => throw new ArgumentOutOfRangeException(nameof(style)),
            };
        }

        private const byte ActionRowType = 1;
        private const byte ButtonType = 2;

        public async ValueTask SendFollowupResponseAsync(ApplicationCommand interaction, Embed embed, IReadOnlyList<Button>? buttons = null)
        {
            var applicationInfo = await _taylorBotClient.Value.DiscordShardedClient.GetApplicationInfoAsync();

            var response = await _httpClient.PostAsync(
                $"https://discord.com/api/v8/webhooks/{applicationInfo.Id}/{interaction.Token}",
                JsonContent.Create(new InteractionResponse.InteractionApplicationCommandCallbackData(
                    content: string.Empty,
                    embeds: new[] { ToInteractionEmbed(embed) },
                    components: buttons == null ? null : buttons.Count < 1 ? Array.Empty<InteractionResponse.Component>() : new[] {
                        new InteractionResponse.Component(ActionRowType, components: buttons.Select(b =>
                            new InteractionResponse.Component(ButtonType, label: b.Label, style: (byte)ToInteractionStyle(b.Style), custom_id: b.Id)
                        ).ToList()
                    ) }
                ))
            );

            response.EnsureSuccessStatusCode();
        }

        public async ValueTask EditOriginalResponseAsync(ButtonComponent component, Embed embed, IReadOnlyList<Button>? buttons = null)
        {
            var applicationInfo = await _taylorBotClient.Value.DiscordShardedClient.GetApplicationInfoAsync();

            var response = await _httpClient.PatchAsync(
                $"https://discord.com/api/v8/webhooks/{applicationInfo.Id}/{component.Token}/messages/@original",
                JsonContent.Create(new InteractionResponse.InteractionApplicationCommandCallbackData(
                    content: string.Empty,
                    embeds: new[] { ToInteractionEmbed(embed) },
                    components: buttons == null ? null : buttons.Count < 1 ? Array.Empty<InteractionResponse.Component>() : new[] {
                        new InteractionResponse.Component(ActionRowType, components: buttons.Select(b =>
                            new InteractionResponse.Component(ButtonType, label: b.Label, style: (byte)ToInteractionStyle(b.Style), custom_id: b.Id)
                        ).ToList()
                    ) }
                ))
            );

            response.EnsureSuccessStatusCode();
        }
    }
}
