using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands
{
    public class BirthdayAgeSlashCommand : ISlashCommand<BirthdayAgeSlashCommand.Options>
    {
        public ISlashCommandInfo Info => new MessageCommandInfo("birthday age");

        public record Options(ParsedUserOrAuthor user);

        private readonly IBirthdayRepository _birthdayRepository;

        public BirthdayAgeSlashCommand(IBirthdayRepository birthdayRepository)
        {
            _birthdayRepository = birthdayRepository;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new Command(
                new(Info.Name),
                async () =>
                {
                    var user = options.user.User;
                    var birthday = await _birthdayRepository.GetBirthdayAsync(user);

                    if (birthday != null)
                    {
                        if (birthday.Date.Year != IBirthdayRepository.Birthday.NoYearValue)
                        {
                            var age = AgeCalculator.GetCurrentAge(context.CreatedAt, birthday.Date);

                            var embed = new EmbedBuilder()
                                .WithUserAsAuthor(user)
                                .WithColor(TaylorBotColors.SuccessColor)
                                .WithTitle("Age")
                                .WithDescription($"{user.Username} is **{age}** years old.");

                            return new EmbedResult(embed.Build());
                        }
                        else
                        {
                            return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                                $"{user.Mention}'s birthday was set without a year so I can't tell how old they are. 🚫",
                                $"Use {context.MentionCommand("birthday set")} with the **year** option, your age will automatically update and you will get taypoints on your birthday every year! 🎈",
                                $"Don't want to share your exact birthday, but want the points, horoscope and age commands? Use the **privately** option. 🕵️",
                            })));
                        }
                    }
                    else
                    {
                        return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                            $"{user.Mention}'s birthday is not set so I can't tell how old they are. 🚫",
                            $"They need to use {context.MentionCommand("birthday set")} to set it first.",
                        })));
                    }
                }
            ));
        }
    }
}
