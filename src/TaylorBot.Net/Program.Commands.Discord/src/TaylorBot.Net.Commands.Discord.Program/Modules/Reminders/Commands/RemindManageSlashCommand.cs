using Humanizer;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Commands
{
    public class RemindManageSlashCommand : ISlashCommand<NoOptions>
    {
        public SlashCommandInfo Info => new("remind manage", IsPrivateResponse: true);

        private readonly IReminderRepository _reminderRepository;

        public RemindManageSlashCommand(IReminderRepository reminderRepository)
        {
            _reminderRepository = reminderRepository;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
        {
            return new(new Command(
                new(Info.Name),
                async () =>
                {
                    var reminders = await _reminderRepository.GetRemindersAsync(context.User);

                    var reminderViews = reminders.Select((reminder, i) => new
                    {
                        Domain = reminder,
                        UserFacingId = i + 1,
                        Summary = $"{reminder.Text.Replace("\n", " ").Truncate(75)} ({reminder.RemindAt.Humanize(culture: TaylorBotCulture.Culture)})"
                    }).ToList();

                    var content = reminderViews.Count > 0 ?
                        string.Join("\n", reminderViews.Select(r => $"**{r.UserFacingId}:** {r.Summary}")) :
                        string.Join("\n", new[] {
                            "You don't have any reminders. 😶",
                            $"Add one with {context.MentionCommand("remind add")}."
                        });

                    var clearButtons = reminderViews.Select(r => new MessageResult.ButtonResult(
                        new Button(Id: $"{r.UserFacingId}-clear", ButtonStyle.Danger, Label: $"Clear #{r.UserFacingId}", Emoji: "🗑"),
                        async () =>
                        {
                            await _reminderRepository.ClearReminderAsync(r.Domain);
                            return new(new MessageContent(EmbedFactory.CreateSuccess($"Reminder {r.UserFacingId} has been cleared. 👍")));
                        }
                    ));

                    var clearAllButton = new MessageResult.ButtonResult(
                        new Button(Id: "clearall", ButtonStyle.Danger, Label: "Clear all", Emoji: "🗑"),
                        async () =>
                        {
                            await _reminderRepository.ClearAllRemindersAsync(context.User);
                            return new(new MessageContent(EmbedFactory.CreateSuccess("All your reminders have been cleared. 👍")));
                        }
                    );

                    return new MessageResult(
                        new(EmbedFactory.CreateSuccess(content)),
                        reminderViews.Count > 0 ? clearButtons.Append(clearAllButton).ToList() : null
                    );
                }
            ));
        }
    }
}
