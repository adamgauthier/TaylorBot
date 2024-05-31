using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Time;
using static TaylorBot.Net.Commands.MessageResult;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Commands;

public class RemindManageSlashCommand(IReminderRepository reminderRepository) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("remind manage", IsPrivateResponse: true);

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
                    Summary = $"{reminder.Text.Replace("\n", " ").Truncate(75)} ({reminder.RemindAt.FormatRelative()})"
                }).ToList();

                var content = reminderViews.Count > 0 ?
                    string.Join("\n", reminderViews.Select(r => $"{r.UserFacingId}. {r.Summary}")) :
                    $"""
                    You don't have any reminders. 😶
                    Add one with {context.MentionCommand("remind add")}.
                    """;

                var clearButtons = reminderViews.Select(r => new ButtonResult(
                    new Button(Id: $"{r.UserFacingId}-clear", ButtonStyle.Danger, Label: $"Clear #{r.UserFacingId}", Emoji: "🗑"),
                    async (_) =>
                    {
                        await reminderRepository.ClearReminderAsync(r.Domain);
                        return new UpdateMessage(new(new(EmbedFactory.CreateSuccess($"Reminder {r.UserFacingId} has been cleared. 👍"))));
                    }
                ));

                var clearAllButton = new ButtonResult(
                    new Button(Id: "clearall", ButtonStyle.Danger, Label: "Clear all", Emoji: "🗑"),
                    async (_) =>
                    {
                        await reminderRepository.ClearAllRemindersAsync(context.User);
                        return new UpdateMessage(new(new(EmbedFactory.CreateSuccess("All your reminders have been cleared. 👍"))));
                    }
                );

                return new MessageResult(
                    new(new EmbedBuilder()
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithTitle("Your Reminders ⏲️")
                        .WithDescription(content)
                        .Build()),
                    reminderViews.Count > 0 ? new(clearButtons.Append(clearAllButton).ToList()) : null
                );
            }
        ));
    }
}
