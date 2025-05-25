using TaylorBot.Net.Core.Client;

namespace TaylorBot.Net.Commands.PostExecution;

public sealed record InteractionResponse(byte type, InteractionResponse.InteractionApplicationCommandCallbackData? data)
{
    public sealed record InteractionApplicationCommandCallbackData(
        string? content = null,
        IReadOnlyList<DiscordEmbed>? embeds = null,
        byte? flags = null,
        IReadOnlyList<Component>? components = null,
        IReadOnlyList<Attachment>? attachments = null,
        string? custom_id = null,
        string? title = null
    );

    public sealed record Component(
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
        IReadOnlyList<byte>? channel_types = null,
        IReadOnlyList<SelectDefaultValue>? default_values = null
    )
    {
        public static Component CreateActionRow(params IReadOnlyList<Component> components)
        {
            return new Component(1, components: components);
        }

        public static Component CreateButton(InteractionButtonStyle style, string label, string custom_id, Emoji? emoji = null, bool? disabled = null)
        {
            return new Component(
                2,
                style: (byte)style,
                label: label,
                custom_id: custom_id,
                emoji: emoji,
                disabled: disabled
            );
        }

        public static Component CreateTextInput(string custom_id, InteractionTextInputStyle style, string label, int? min_length = null, int? max_length = null, bool? required = null, string? value = null, string? placeholder = null)
        {
            return new Component(
                4,
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

        public static Component CreateUserSelect(string custom_id, string? placeholder = null, int? min_values = null, int? max_values = null, bool? disabled = null, IReadOnlyList<SelectDefaultValue>? default_values = null)
        {
            return CreateActionRow(new Component(
                5,
                custom_id: custom_id,
                placeholder: placeholder,
                min_length: min_values,
                max_length: max_values,
                disabled: disabled,
                default_values: default_values
            ));
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

    public sealed record Attachment(int id, string filename, string? description = null);

    public sealed record Emoji(string name);

    public sealed record SelectDefaultValue(string id, string type);
}

public enum InteractionButtonStyle
{
    Unknown = 0,
    Primary = 1,
    Secondary = 2,
    Success = 3,
    Danger = 4,
    Link = 5,
}

public enum InteractionTextInputStyle
{
    Unknown = 0,
    Short = 1,
    Paragraph = 2,
}
