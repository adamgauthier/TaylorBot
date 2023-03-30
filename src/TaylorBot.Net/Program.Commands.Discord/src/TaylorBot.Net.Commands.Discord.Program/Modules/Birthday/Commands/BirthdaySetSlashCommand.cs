using Discord;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands
{
    public class BirthdaySetSlashCommand : ISlashCommand<BirthdaySetSlashCommand.Options>
    {
        public ISlashCommandInfo Info => new MessageCommandInfo("birthday set", IsPrivateResponse: true);

        public record Options(ParsedPositiveInteger day, ParsedPositiveInteger month, ParsedOptionalInteger year, ParsedOptionalBoolean privately);

        private const int MinAge = 13;
        private const int MaxAge = 115;
        private readonly IBirthdayRepository _birthdayRepository;

        public BirthdaySetSlashCommand(IBirthdayRepository birthdayRepository)
        {
            _birthdayRepository = birthdayRepository;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new Command(
                new(Info.Name),
                async () =>
                {
                    DateOnly birthday = new(options.year.Value ?? IBirthdayRepository.Birthday.NoYearValue, options.month.Value, options.day.Value);

                    if (birthday.Year != IBirthdayRepository.Birthday.NoYearValue)
                    {
                        int age = AgeCalculator.GetCurrentAge(context.CreatedAt, birthday);

                        if (age < MinAge)
                        {
                            return new EmbedResult(EmbedFactory.CreateError($"Age must be higher or equal to {MinAge} years old."));
                        }

                        if (age > MaxAge)
                        {
                            return new EmbedResult(EmbedFactory.CreateError($"Age must be lower or equal to {MaxAge} years old."));
                        }
                    }

                    var isPrivate = options.privately.Value ?? false;

                    if (birthday.Year != IBirthdayRepository.Birthday.NoYearValue)
                    {
                        await _birthdayRepository.ClearLegacyAgeAsync(context.User);
                    }

                    await _birthdayRepository.SetBirthdayAsync(context.User, new(birthday, isPrivate));

                    var embed = new EmbedBuilder()
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription(string.Join('\n', new[] {
                            $"Your birthday has been set **{birthday.ToString("MMMM d", TaylorBotCulture.Culture)}**. ✅",
                            birthday.Year == IBirthdayRepository.Birthday.NoYearValue ?
                                $"Please consider setting your birthday with the **year** option for {context.MentionCommand("birthday age")} to work. ❓" :
                                $"You can now use {context.MentionCommand("birthday age")} to display your age. 🔢",
                            isPrivate ?
                                $"Since your birthday is private, it won't show up in {context.MentionCommand("birthday calendar")}. 🙈" :
                                $"Your birthday will show up in {context.MentionCommand("birthday calendar")}. 📅",
                            $"You can now use {context.MentionCommand("birthday horoscope")} to get your horoscope. ✨",
                            "You will get taypoints on your birthday every year. 🎁",
                        }));

                    return new EmbedResult(embed.Build());
                }
            ));
        }
    }
}
