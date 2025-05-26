using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Http;

namespace TaylorBot.Net.Commands.PostExecution;

public interface IInteractionResponseClient
{
    ValueTask SendImmediateResponseAsync(ApplicationCommand command, MessageResponse message);
    ValueTask SendAckResponseWithLoadingMessageAsync(ApplicationCommand command, bool isEphemeral = false);
    ValueTask SendAckResponseWithLoadingMessageAsync(ModalSubmit submit, bool isEphemeral = false);
    ValueTask SendComponentAckResponseWithoutLoadingMessageAsync(IDiscordMessageComponent messageComponent);
    ValueTask SendModalResponseAsync(DiscordButtonComponent button, CreateModalResult createModal);
    ValueTask SendModalResponseAsync(ApplicationCommand command, CreateModalResult createModal);
    ValueTask SendFollowupResponseAsync(ParsedInteraction interaction, MessageResponse message);
    ValueTask SendFollowupResponseAsync(string token, MessageResponse message);
    ValueTask EditOriginalResponseAsync(ParsedInteraction interaction, MessageResponse message);
    ValueTask EditOriginalResponseAsync(ParsedInteraction interaction, DiscordEmbed embed);
    ValueTask DeleteOriginalResponseAsync(DiscordButtonComponent component);
    ValueTask PatchComponentsAsync(ParsedInteraction interaction, IReadOnlyList<InteractionComponent> components);
}

public class InteractionResponseClient(ILogger<IInteractionResponseClient> logger, Lazy<ITaylorBotClient> taylorBotClient, HttpClient httpClient) : IInteractionResponseClient
{
    private const byte ChannelMessageWithSourceInteractionResponseType = 4;
    private const byte DeferredChannelMessageWithSourceInteractionResponseType = 5;
    private const byte ComponentDeferredUpdateMessageInteractionResponseType = 6;
    private const byte ModalInteractionResponseType = 9;

    public async ValueTask SendImmediateResponseAsync(ApplicationCommand command, MessageResponse message)
    {
        using var content = JsonContent.Create(new InteractionResponse(ChannelMessageWithSourceInteractionResponseType, ToInteractionData(message)));
        var response = await httpClient.PostAsync(
            $"interactions/{command.Interaction.Id}/{command.Interaction.Token}/callback",
            content
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
        using var content = JsonContent.Create(new InteractionResponse(DeferredChannelMessageWithSourceInteractionResponseType, isEphemeral ? new(flags: 64) : null));
        var response = await httpClient.PostAsync(
            $"interactions/{id}/{token}/callback",
            content
        );
        await response.EnsureSuccessAsync(logger);
    }

    public async ValueTask SendComponentAckResponseWithoutLoadingMessageAsync(IDiscordMessageComponent messageComponent)
    {
        using var content = JsonContent.Create(new InteractionResponse(ComponentDeferredUpdateMessageInteractionResponseType, null));
        var response = await httpClient.PostAsync(
            $"interactions/{messageComponent.Interaction.Id}/{messageComponent.Interaction.Token}/callback",
            content
        );
        await response.EnsureSuccessAsync(logger);
    }

    private async ValueTask SendModalResponseAsync(string id, string token, CreateModalResult createModal)
    {
        // Text inputs fill an entire ActionRow
        var components = createModal.TextInputs.Select(t =>
            InteractionComponent.CreateActionRow(
                InteractionComponent.CreateTextInput(
                    custom_id: t.Id,
                    style: ToInteractionStyle(t.Style),
                    label: t.Label,
                    min_length: t.MinLength,
                    max_length: t.MaxLength,
                    required: t.Required
                )
            )
        ).ToList();

        InteractionResponse interactionResponse = new(
            ModalInteractionResponseType,
            new(custom_id: createModal.Id, title: createModal.Title, components: components)
        );

        using var content = JsonContent.Create(interactionResponse);
        var response = await httpClient.PostAsync(
            $"interactions/{id}/{token}/callback",
            content
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
            embeds: [.. response.Content.Embeds.Select(InteractionMapper.ToInteractionEmbed)],
            components: response.Components,
            attachments: response.Content.Attachments?.Select((a, i) => new InteractionResponse.Attachment(
                id: i,
                filename: a.Filename
            )).ToList(),
            flags: response.IsPrivate ? 64 : null
        );
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

        using var jsonContent = JsonContent.Create(ToInteractionData(message));

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
        await PatchOriginalResponseAsync(token, data);
    }

    private async ValueTask PatchOriginalResponseAsync(string token, object data)
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

    public async ValueTask PatchComponentsAsync(ParsedInteraction interaction, IReadOnlyList<InteractionComponent> components)
    {
        await PatchOriginalResponseAsync(interaction.Token, new { components });
    }
}
