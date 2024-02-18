using Discord;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Json;
using TaylorBot.Net.Core.Client;

namespace TaylorBot.Net.Commands.PostExecution;

public class InteractionResponseClient(ILogger<InteractionResponseClient> logger, Lazy<ITaylorBotClient> taylorBotClient, HttpClient httpClient)
{
    private const byte ChannelMessageWithSourceInteractionResponseType = 4;
    private const byte DeferredChannelMessageWithSourceInteractionResponseType = 5;
    private const byte ComponentDeferredUpdateMessageInteractionResponseType = 6;
    private const byte ModalInteractionResponseType = 9;

    private record InteractionResponse(byte type, InteractionResponse.InteractionApplicationCommandCallbackData? data)
    {
        public record InteractionApplicationCommandCallbackData(
            string? content = null,
            IReadOnlyList<Embed>? embeds = null,
            byte? flags = null,
            IReadOnlyList<Component>? components = null,
            IReadOnlyList<Attachment>? attachments = null,
            string? custom_id = null,
            string? title = null
        );

        public record Component(
            byte type,
            byte? style = null,
            string? label = null,
            Emoji? emoji = null,
            string? custom_id = null,
            string? url = null,
            bool? disabled = null,
            int? min_length = null,
            int? max_length = null,
            bool? required = null,
            string? value = null,
            string? placeholder = null,
            IReadOnlyList<Component>? components = null
        )
        {
            public static Component CreateActionRow(IReadOnlyList<Component> components)
            {
                return new Component(1, components: components);
            }

            public static Component CreateButton(byte style, string label, string custom_id, Emoji? emoji = null, bool? disabled = null)
            {
                return new Component(
                    2,
                    style: style,
                    label: label,
                    custom_id: custom_id,
                    emoji: emoji,
                    disabled: disabled
                );
            }

            public static Component CreateTextInput(string custom_id, byte style, string label, int? min_length = null, int? max_length = null, bool? required = null, string? value = null, string? placeholder = null)
            {
                return new Component(
                    4,
                    style: style,
                    label: label,
                    custom_id: custom_id,
                    min_length: min_length,
                    max_length: max_length,
                    required: required,
                    value: value,
                    placeholder: placeholder
                );
            }
        };

        public record Attachment(int id, string filename, string? description = null);

        public record Embed(string? title, string? description, string? url, EmbedAuthor? author, EmbedImage? image, EmbedThumbnail? thumbnail, uint? color, EmbedFooter? footer, IReadOnlyList<EmbedField>? fields, string? timestamp);

        public record EmbedAuthor(string? name, string? url, string? icon_url);

        public record EmbedImage(string url);

        public record EmbedThumbnail(string url);

        public record EmbedFooter(string text, string? icon_url, string? proxy_icon_url);

        public record EmbedField(string name, string value, bool? inline);

        public record Emoji(string name);
    }

    private enum InteractionButtonStyle { Primary = 1, Secondary = 2, Success = 3, Danger = 4, Link = 5 }
    private enum InteractionTextInputStyle { Short = 1, Paragraph = 2 }

    public async ValueTask SendImmediateResponseAsync(ApplicationCommand interaction, MessageResponse message)
    {
        var response = await httpClient.PostAsync(
            $"interactions/{interaction.Id}/{interaction.Token}/callback",
            JsonContent.Create(new InteractionResponse(ChannelMessageWithSourceInteractionResponseType, ToInteractionData(message)))
        );

        await EnsureSuccess(response);
    }

    public async ValueTask SendAckResponseWithLoadingMessageAsync(ApplicationCommand interaction, bool isEphemeral = false)
    {
        await SendAckResponseWithLoadingMessageAsync(interaction.Id, interaction.Token, isEphemeral);
    }

    public async ValueTask SendAckResponseWithLoadingMessageAsync(ModalSubmit interaction, bool isEphemeral = false)
    {
        await SendAckResponseWithLoadingMessageAsync(interaction.Id, interaction.Token, isEphemeral);
    }

    private async ValueTask SendAckResponseWithLoadingMessageAsync(string id, string token, bool isEphemeral)
    {
        var response = await httpClient.PostAsync(
            $"interactions/{id}/{token}/callback",
            JsonContent.Create(new InteractionResponse(DeferredChannelMessageWithSourceInteractionResponseType, isEphemeral ? new(flags: 64) : null))
        );

        await EnsureSuccess(response);
    }

    public async ValueTask SendComponentAckResponseWithoutLoadingMessageAsync(ButtonComponent interaction)
    {
        var response = await httpClient.PostAsync(
            $"interactions/{interaction.Id}/{interaction.Token}/callback",
            JsonContent.Create(new InteractionResponse(ComponentDeferredUpdateMessageInteractionResponseType, null))
        );

        await EnsureSuccess(response);
    }

    public async ValueTask SendModalResponseAsync(ApplicationCommand interaction, CreateModalResult createModal)
    {
        // Text inputs fill an entire ActionRow
        var components = createModal.TextInputs.Select(t =>
            InteractionResponse.Component.CreateActionRow([
                 InteractionResponse.Component.CreateTextInput(
                    custom_id: t.Id,
                    style: (byte)ToInteractionStyle(t.Style),
                    label: t.Label,
                    min_length: t.MinLength,
                    max_length: t.MaxLength,
                    required: t.Required
                )
            ])
        ).ToList();

        InteractionResponse interactionResponse = new(
            ModalInteractionResponseType,
            new(custom_id: createModal.Id, title: createModal.Title, components: components)
        );

        var response = await httpClient.PostAsync(
            $"interactions/{interaction.Id}/{interaction.Token}/callback",
            JsonContent.Create(interactionResponse)
        );

        await EnsureSuccess(response);
    }

    private static InteractionResponse.Embed ToInteractionEmbed(Embed embed)
    {
        return new InteractionResponse.Embed(
            title: embed.Title,
            description: embed.Description,
            url: embed.Url,
            author: embed.Author.HasValue ? new(embed.Author.Value.Name, embed.Author.Value.Url, embed.Author.Value.IconUrl) : null,
            image: embed.Image.HasValue ? new(embed.Image.Value.Url) : null,
            thumbnail: embed.Thumbnail.HasValue ? new(embed.Thumbnail.Value.Url) : null,
            color: embed.Color.HasValue ? embed.Color.Value.RawValue : null,
            footer: embed.Footer.HasValue ? new(embed.Footer.Value.Text, embed.Footer.Value.IconUrl, embed.Footer.Value.ProxyUrl) : null,
            fields: embed.Fields.Select(f => new InteractionResponse.EmbedField(f.Name, f.Value, f.Inline)).ToList(),
            timestamp: embed.Timestamp.HasValue ? embed.Timestamp.Value.ToString("s", CultureInfo.InvariantCulture) : null
        );
    }

    private static InteractionResponse.InteractionApplicationCommandCallbackData ToInteractionData(MessageResponse response)
    {
        return new InteractionResponse.InteractionApplicationCommandCallbackData(
            content: response.Content.Content,
            embeds: response.Content.Embeds.Select(ToInteractionEmbed).ToList(),
            components: ToInteractionComponents(response),
            attachments: response.Content.Attachments?.Select((a, i) => new InteractionResponse.Attachment(
                id: i,
                filename: a.Filename
            )).ToList(),
            flags: response.IsPrivate ? 64 : null
        );
    }

    private static InteractionResponse.Component[]? ToInteractionComponents(MessageResponse response)
    {
        if (response.Buttons != null)
        {
            if (!response.Buttons.Any())
            {
                return Array.Empty<InteractionResponse.Component>();
            }

            return [
                InteractionResponse.Component.CreateActionRow(response.Buttons.Select(b =>
                    InteractionResponse.Component.CreateButton(
                        style: (byte)ToInteractionStyle(b.Style),
                        label: b.Label,
                        custom_id: b.Id,
                        emoji: b.Emoji != null ? new(name: b.Emoji) : null
                    )
                ).ToList())
            ];
        }
        else
        {
            return null;
        }
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

    private static InteractionTextInputStyle ToInteractionStyle(TextInputStyle style)
    {
        return style switch
        {
            TextInputStyle.Short => InteractionTextInputStyle.Short,
            TextInputStyle.Paragraph => InteractionTextInputStyle.Paragraph,
            _ => throw new ArgumentOutOfRangeException(nameof(style)),
        };
    }

    private static MultipartFormDataContent CreateContentWithAttachments(IReadOnlyList<Attachment> attachments, JsonContent jsonContent)
    {
        var content = new MultipartFormDataContent
        {
            { jsonContent, "payload_json" },
        };

        foreach (var (attachment, i) in attachments.Select((item, index) => (item, index)))
        {
            content.Add(new StreamContent(attachment.Stream), $"files[{i}]", attachment.Filename);
        }

        return content;
    }

    public async ValueTask SendFollowupResponseAsync(ButtonComponent component, MessageResponse message)
    {
        await SendFollowupResponseAsync(component.Token, message);
    }

    public async ValueTask SendFollowupResponseAsync(ApplicationCommand interaction, MessageResponse message)
    {
        await SendFollowupResponseAsync(interaction.Token, message);
    }

    public async ValueTask SendFollowupResponseAsync(string token, MessageResponse message)
    {
        var applicationInfo = await taylorBotClient.Value.DiscordShardedClient.GetApplicationInfoAsync();

        var jsonContent = JsonContent.Create(ToInteractionData(message));

        using HttpContent httpContent = message.Content.Attachments?.Any() == true ?
            CreateContentWithAttachments(message.Content.Attachments, jsonContent) :
            jsonContent;

        var response = await httpClient.PostAsync(
            $"webhooks/{applicationInfo.Id}/{token}",
            httpContent
        );

        await EnsureSuccess(response);
    }

    public async ValueTask EditOriginalResponseAsync(IInteraction interaction, MessageResponse message)
    {
        await EditOriginalResponseAsync(interaction.Token, message);
    }

    private async ValueTask EditOriginalResponseAsync(string token, MessageResponse message)
    {
        var applicationInfo = await taylorBotClient.Value.DiscordShardedClient.GetApplicationInfoAsync();

        using var content = JsonContent.Create(ToInteractionData(message));

        var response = await httpClient.PatchAsync(
            $"webhooks/{applicationInfo.Id}/{token}/messages/@original",
            content
        );

        await EnsureSuccess(response);
    }

    public async ValueTask DeleteOriginalResponseAsync(ButtonComponent component)
    {
        var applicationInfo = await taylorBotClient.Value.DiscordShardedClient.GetApplicationInfoAsync();

        var response = await httpClient.DeleteAsync(
            $"webhooks/{applicationInfo.Id}/{component.Token}/messages/@original"
        );

        await EnsureSuccess(response);
    }

    private async ValueTask EnsureSuccess(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                logger.LogError("Error response from Discord ({StatusCode}): {Body}", response.StatusCode, body);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Unhandled error when parsing error body ({StatusCode}):", response.StatusCode);
            }

            response.EnsureSuccessStatusCode();
        }
    }
}
