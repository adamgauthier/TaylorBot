using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Http;

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
            IReadOnlyList<DiscordEmbed>? embeds = null,
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
            IReadOnlyList<Component>? components = null,
            IReadOnlyList<byte>? channel_types = null
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

            public static Component CreateChannelSelectMenu(string custom_id, IReadOnlyList<byte>? channel_types, string? placeholder = null, bool? disabled = null)
            {
                return CreateActionRow([new Component(
                    8,
                    channel_types: channel_types,
                    custom_id: custom_id,
                    placeholder: placeholder,
                    disabled: disabled
                )]);
            }
        };

        public record Attachment(int id, string filename, string? description = null);

        public record Emoji(string name);
    }

    private enum InteractionButtonStyle { Primary = 1, Secondary = 2, Success = 3, Danger = 4, Link = 5 }
    private enum InteractionTextInputStyle { Short = 1, Paragraph = 2 }

    public async ValueTask SendImmediateResponseAsync(ApplicationCommand command, MessageResponse message)
    {
        var response = await httpClient.PostAsync(
            $"interactions/{command.Interaction.Id}/{command.Interaction.Token}/callback",
            JsonContent.Create(new InteractionResponse(ChannelMessageWithSourceInteractionResponseType, ToInteractionData(message)))
        );
        await response.EnsureSuccessAsync(logger);
    }

    public async ValueTask SendAckResponseWithLoadingMessageAsync(ApplicationCommand command, bool isEphemeral = false)
    {
        await SendAckResponseWithLoadingMessageAsync(command.Interaction.Id, command.Interaction.Token, isEphemeral);
    }

    public async ValueTask SendAckResponseWithLoadingMessageAsync(ModalSubmit submit, bool isEphemeral = false)
    {
        await SendAckResponseWithLoadingMessageAsync(submit.Interaction.Id, submit.Token, isEphemeral);
    }

    private async ValueTask SendAckResponseWithLoadingMessageAsync(string id, string token, bool isEphemeral)
    {
        var response = await httpClient.PostAsync(
            $"interactions/{id}/{token}/callback",
            JsonContent.Create(new InteractionResponse(DeferredChannelMessageWithSourceInteractionResponseType, isEphemeral ? new(flags: 64) : null))
        );
        await response.EnsureSuccessAsync(logger);
    }

    public async ValueTask SendComponentAckResponseWithoutLoadingMessageAsync(DiscordButtonComponent button)
    {
        var response = await httpClient.PostAsync(
            $"interactions/{button.Interaction.Id}/{button.Interaction.Token}/callback",
            JsonContent.Create(new InteractionResponse(ComponentDeferredUpdateMessageInteractionResponseType, null))
        );
        await response.EnsureSuccessAsync(logger);
    }

    private async ValueTask SendModalResponseAsync(string id, string token, CreateModalResult createModal)
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
            $"interactions/{id}/{token}/callback",
            JsonContent.Create(interactionResponse)
        );
        await response.EnsureSuccessAsync(logger);
    }

    public async ValueTask SendModalResponseAsync(DiscordButtonComponent button, CreateModalResult createModal)
    {
        await SendModalResponseAsync(button.Interaction.Id, button.Interaction.Token, createModal);
    }

    public async ValueTask SendModalResponseAsync(ApplicationCommand command, CreateModalResult createModal)
    {
        await SendModalResponseAsync(command.Interaction.Id, command.Interaction.Token, createModal);
    }

    private static InteractionResponse.InteractionApplicationCommandCallbackData ToInteractionData(MessageResponse response)
    {
        return new InteractionResponse.InteractionApplicationCommandCallbackData(
            content: response.Content.Content,
            embeds: response.Content.Embeds.Select(InteractionMapper.ToInteractionEmbed).ToList(),
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
            if (response.Buttons.Count == 0)
            {
                return [];
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

    public async ValueTask SendFollowupResponseAsync(ParsedInteraction interaction, MessageResponse message)
    {
        await SendFollowupResponseAsync(interaction.Token, message);
    }

    public async ValueTask SendFollowupResponseAsync(string token, MessageResponse message)
    {
        var applicationInfo = await taylorBotClient.Value.DiscordShardedClient.GetApplicationInfoAsync();

        var jsonContent = JsonContent.Create(ToInteractionData(message));

        using HttpContent httpContent = message.Content.Attachments?.Count > 0 ?
            CreateContentWithAttachments(message.Content.Attachments, jsonContent) :
            jsonContent;

        var response = await httpClient.PostAsync(
            $"webhooks/{applicationInfo.Id}/{token}",
            httpContent
        );
        await response.EnsureSuccessAsync(logger);
    }

    public async ValueTask EditOriginalResponseAsync(ParsedInteraction interaction, MessageResponse message)
    {
        var data = ToInteractionData(message);
        await EditOriginalResponseAsync(interaction.Token, data);
    }

    public async ValueTask EditOriginalResponseAsync(ParsedInteraction interaction, DiscordEmbed embed)
    {
        InteractionResponse.InteractionApplicationCommandCallbackData data = new(embeds: [embed]);
        await EditOriginalResponseAsync(interaction.Token, data);
    }

    private async ValueTask EditOriginalResponseAsync(string token, InteractionResponse.InteractionApplicationCommandCallbackData data)
    {
        var applicationInfo = await taylorBotClient.Value.DiscordShardedClient.GetApplicationInfoAsync();

        using var content = JsonContent.Create(data);

        var response = await httpClient.PatchAsync(
            $"webhooks/{applicationInfo.Id}/{token}/messages/@original",
            content
        );
        await response.EnsureSuccessAsync(logger);
    }

    public async ValueTask DeleteOriginalResponseAsync(DiscordButtonComponent component)
    {
        var applicationInfo = await taylorBotClient.Value.DiscordShardedClient.GetApplicationInfoAsync();

        var response = await httpClient.DeleteAsync(
            $"webhooks/{applicationInfo.Id}/{component.Interaction.Token}/messages/@original"
        );
        await response.EnsureSuccessAsync(logger);
    }
}
