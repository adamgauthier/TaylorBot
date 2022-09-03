using Humanizer;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Commands
{
    public class DailyStreakSlashCommand : ISlashCommand<DailyStreakSlashCommand.Options>
    {
        public SlashCommandInfo Info => new("daily streak");

        public record Options(ParsedUserOrAuthor user);

        private readonly IDailyPayoutRepository _dailyPayoutRepository;

        public DailyStreakSlashCommand(IDailyPayoutRepository dailyPayoutRepository)
        {
            _dailyPayoutRepository = dailyPayoutRepository;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new Command(
                new(Info.Name),
                async () =>
                {
                    var streakInfo = await _dailyPayoutRepository.GetStreakInfoAsync(options.user.User);

                    if (!streakInfo.HasValue)
                    {
                        return new EmbedResult(EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                            $"{options.user.User.Mention} has never claimed their daily reward! ❌",
                            "Use </daily claim:870731803739168859> to claim your daily reward!"
                        })));
                    }
                    else if (streakInfo.Value.MaxStreak > streakInfo.Value.CurrentStreak)
                    {
                        return new EmbedResult(EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                            $"{options.user.User.Mention}'s current streak is {"day".ToQuantity(streakInfo.Value.CurrentStreak, TaylorBotFormats.BoldReadable)}! ⭐",
                            $"Their highest streak ever is {"day".ToQuantity(streakInfo.Value.MaxStreak, TaylorBotFormats.BoldReadable)}! 🥇",
                        })));
                    }
                    else
                    {
                        return new EmbedResult(EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                            $"{options.user.User.Mention}'s current streak is the highest it's ever been ({"day".ToQuantity(streakInfo.Value.CurrentStreak, TaylorBotFormats.BoldReadable)})! ⭐",
                        })));
                    }
                }
            ));
        }
    }
}

