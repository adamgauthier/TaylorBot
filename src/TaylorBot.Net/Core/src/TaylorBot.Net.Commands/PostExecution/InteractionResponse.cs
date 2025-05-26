using TaylorBot.Net.Core.Client;
using static TaylorBot.Net.Commands.PostExecution.InteractionResponse;

namespace TaylorBot.Net.Commands.PostExecution;

public sealed record InteractionResponse(byte type, InteractionApplicationCommandCallbackData? data)
{
    public sealed record InteractionApplicationCommandCallbackData(
        string? content = null,
        IReadOnlyList<DiscordEmbed>? embeds = null,
        byte? flags = null,
        IReadOnlyList<InteractionComponent>? components = null,
        IReadOnlyList<Attachment>? attachments = null,
        string? custom_id = null,
        string? title = null
    );

    public sealed record Attachment(int id, string filename, string? description = null);

    public sealed record Emoji(string name);

    public sealed record SelectDefaultValue(string id, string type);
}

public sealed record InteractionComponent(
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
    IReadOnlyList<InteractionComponent>? components = null,
    IReadOnlyList<byte>? channel_types = null,
    IReadOnlyList<SelectDefaultValue>? default_values = null
)
{
    public static InteractionComponent CreateActionRow(params IReadOnlyList<InteractionComponent> components)
    {
        return new InteractionComponent((byte)InteractionComponentType.ActionRow, components: components);
    }

    public static InteractionComponent CreateButton(InteractionButtonStyle style, string label, string custom_id, Emoji? emoji = null, bool? disabled = null)
    {
        return new InteractionComponent(
            (byte)InteractionComponentType.Button,
            style: (byte)style,
            label: label,
            custom_id: custom_id,
            emoji: emoji,
            disabled: disabled
        );
    }

    public static InteractionComponent CreateTextInput(string custom_id, InteractionTextInputStyle style, string label, int? min_length = null, int? max_length = null, bool? required = null, string? value = null, string? placeholder = null)
    {
        return new InteractionComponent(
            (byte)InteractionComponentType.TextInput,
            style: (byte)style,
            label: label,
            custom_id: custom_id,
            min_length: min_length,
            max_length: max_length,
            required: required,
            value: value,
            placeholder: placeholder
        );
    }

    public static InteractionComponent CreateUserSelect(string custom_id, string? placeholder = null, int? min_values = null, int? max_values = null, bool? disabled = null, IReadOnlyList<SelectDefaultValue>? default_values = null)
    {
        return CreateActionRow(new InteractionComponent(
            (byte)InteractionComponentType.UserSelect,
            custom_id: custom_id,
            placeholder: placeholder,
            min_length: min_values,
            max_length: max_values,
            disabled: disabled,
            default_values: default_values
        ));
    }

    public static InteractionComponent CreateChannelSelectMenu(string custom_id, IReadOnlyList<byte>? channel_types, string? placeholder = null, bool? disabled = null)
    {
        return CreateActionRow([new InteractionComponent(
            (byte)InteractionComponentType.ChannelSelect,
            channel_types: channel_types,
            custom_id: custom_id,
            placeholder: placeholder,
            disabled: disabled
        )]);
    }
}

public enum InteractionComponentType : byte
{
    Unknown = 0,
    ActionRow = 1,
    Button = 2,
    StringSelect = 3,
    TextInput = 4,
    UserSelect = 5,
    RoleSelect = 6,
    MentionableSelect = 7,
    ChannelSelect = 8,
    Section = 9,
    TextDisplay = 10,
    Thumbnail = 11,
    MediaGallery = 12,
    File = 13,
    Separator = 14,
    Container = 17
}

public enum InteractionButtonStyle : byte
{
    Unknown = 0,
    Primary = 1,
    Secondary = 2,
    Success = 3,
    Danger = 4,
    Link = 5,
}

public enum InteractionTextInputStyle : byte
{
    Unknown = 0,
    Short = 1,
    Paragraph = 2,
}
