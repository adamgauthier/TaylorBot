using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Time;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Commands;

public class RemindManageSlashCommand(IReminderRepository reminderRepository, CommandMentioner mention) : ISlashCommand<NoOptions>
{
    public static string CommandName => "remind manage";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName, IsPrivateResponse: true);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var reminders = await reminderRepository.GetRemindersAsync(context.User);

                var reminderViews = reminders.Select((reminder, i) => new
                {
                    Domain = reminder,
                    UserFacingId = i + 1,
                    Summary = $"{reminder.Text.Replace("\n", " ", StringComparison.InvariantCulture).Truncate(75)} ({reminder.RemindAt.FormatRelative()})"
                }).ToList();

                var content = reminderViews.Count > 0 ?
                    string.Join("\n", reminderViews.Select(r => $"{r.UserFacingId}. {r.Summary}")) :
                    $"""
                    You don't have any reminders 😶
                    Add one with {mention.SlashCommand("remind add", context)} ⏲️
                    """;

                var clearButtons = reminderViews.Select(r =>
                {
                    List<CustomIdDataEntry> clearData = [new("rem", $"{r.Domain.Id:N}")];
                    return new Button(
                        Id: InteractionCustomId.Create(RemindManageClearButtonHandler.CustomIdName, clearData).RawId,
                        ButtonStyle.Danger,
                        Label: $"Clear #{r.UserFacingId}",
                        Emoji: "🗑"
                    );
                });
                Button clearAllButton = new(
                    Id: InteractionCustomId.Create(RemindManageClearAllButtonHandler.CustomIdName).RawId,
                    ButtonStyle.Danger,
                    Label: "Clear all",
                    Emoji: "🗑"
                );

                return new MessageResult(new(
                    new(new EmbedBuilder()
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithTitle("Your Reminders ⏲️")
                        .WithDescription(content)
                        .Build()),
                    reminderViews.Count > 0
                        ? clearButtons.Append(clearAllButton).ToList()
                        : null
                ));
            }
        ));
    }
}

public class RemindManageClearButtonHandler(InteractionResponseClient responseClient, IReminderRepository reminderRepository) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.RemindManageClear;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var reminderId = Guid.ParseExact(button.CustomId.ParsedData["rem"], "N");

        await reminderRepository.ClearReminderAsync(reminderId);

        await responseClient.EditOriginalResponseAsync(button.Interaction, message: new(new(EmbedFactory.CreateSuccess($"Reminder has been cleared 👍")), []));
    }
}

public class RemindManageClearAllButtonHandler(InteractionResponseClient responseClient, IReminderRepository reminderRepository) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.RemindManageClearAll;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        await reminderRepository.ClearAllRemindersAsync(context.User);

        await responseClient.EditOriginalResponseAsync(button.Interaction, message: new(new(EmbedFactory.CreateSuccess("All your reminders have been cleared ��")), []));
    }
}
