using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Commands;

public class RemindAddSlashCommand(IReminderRepository reminderRepository, IPlusRepository plusRepository) : ISlashCommand<RemindAddSlashCommand.Options>
{
    private const uint MinMinutes = 1;
    private const uint MaxDays = 365;
    private const uint MaxRemindersNonPlus = 2;
    private const uint MaxRemindersPlus = 4;

    public ISlashCommandInfo Info => new MessageCommandInfo("remind add");

    public record Options(ParsedTimeSpan time, ParsedString text);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                if (options.text.Value.Length > EmbedBuilder.MaxDescriptionLength)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"Sorry, the reminder text can't be longer than {EmbedBuilder.MaxDescriptionLength} characters. 😕"
                    ));
                }

                var fromNow = options.time.Value;

                if (fromNow.TotalMinutes < MinMinutes)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"Sorry, you can't be reminded less than {"minute".ToQuantity(MinMinutes)} in the future. 😕"
                    ));
                }

                if (fromNow.TotalDays > MaxDays)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"Sorry, you can't be reminded more than {"day".ToQuantity(MaxDays)} in the future. 😕"
                    ));
                }

                var maxReminders = await plusRepository.IsActivePlusUserAsync(context.User) ? MaxRemindersPlus : MaxRemindersNonPlus;

                if (await reminderRepository.GetReminderCountAsync(context.User) >= maxReminders)
                {
                    return new EmbedResult(EmbedFactory.CreateError(string.Join("\n", new[] {
                        $"Sorry, you can't have more than {maxReminders} set at the same time. 😕",
                        $"Use {context.MentionCommand("remind manage")} to clear some of your current reminders.",
                        $"By default, you can have at most {MaxRemindersNonPlus}. **TaylorBot Plus** members can have {MaxRemindersPlus}."
                    })));
                }

                var remindAt = DateTimeOffset.Now + fromNow;

                await reminderRepository.AddReminderAsync(context.User, remindAt, options.text.Value);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"Okay, I will remind you in about **{fromNow.Humanize(culture: TaylorBotCulture.Culture)}**. 👍"
                ));
            }
        ));
    }
}
