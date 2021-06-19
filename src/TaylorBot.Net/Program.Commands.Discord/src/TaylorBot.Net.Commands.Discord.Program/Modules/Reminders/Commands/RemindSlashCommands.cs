using Discord;
using Humanizer;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Reminders.Commands
{
    public class RemindAddSlashCommand : ISlashCommand<RemindAddSlashCommand.Options>
    {
        private const uint MinMinutes = 1;
        private const uint MaxDays = 365;
        private const uint MaxReminders = 3;

        public SlashCommandInfo Info => new("remind add");

        public record Options(ParsedTimeSpan time, ParsedString text);

        private readonly IReminderRepository _reminderRepository;

        public RemindAddSlashCommand(IReminderRepository reminderRepository)
        {
            _reminderRepository = reminderRepository;
        }

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

                    if (await _reminderRepository.GetReminderCountAsync(context.User) >= MaxReminders)
                    {
                        return new EmbedResult(EmbedFactory.CreateError(
                            $"Sorry, you can't have more than {MaxReminders} set at the same time. 😕"
                        ));
                    }

                    var remindAt = DateTimeOffset.Now + fromNow;

                    await _reminderRepository.AddReminderAsync(context.User, remindAt, options.text.Value);

                    return new EmbedResult(EmbedFactory.CreateSuccess(
                        $"Okay, I will remind you **{remindAt.Humanize(culture: TaylorBotCulture.Culture)}**. 👍"
                    ));
                }
            ));
        }
    }
}
