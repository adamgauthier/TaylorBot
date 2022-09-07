using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Knowledge.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Knowledge.Commands
{
    [Name("Knowledge ❓")]
    public class KnowledgeModule : TaylorBotModule
    {
        private readonly IZodiacSignRepository _zodiacSignRepository;
        private readonly IHoroscopeClient _horoscopeClient;
        private readonly ICommandRunner _commandRunner;
        private readonly IRateLimiter _rateLimiter;

        public KnowledgeModule(IZodiacSignRepository zodiacSignRepository, IHoroscopeClient horoscopeClient, ICommandRunner commandRunner, IRateLimiter rateLimiter)
        {
            _zodiacSignRepository = zodiacSignRepository;
            _horoscopeClient = horoscopeClient;
            _commandRunner = commandRunner;
            _rateLimiter = rateLimiter;
        }

        [Command("horoscope")]
        [Alias("hs")]
        [Summary("Gets the horoscope of a user based on their set birthday.")]
        public async Task<RuntimeResult> HoroscopeAsync(
            [Summary("What user would you like to see the horoscope of?")]
            [Remainder]
            IUserArgument<IUser>? user = null
        )
        {
            var command = new Command(
                DiscordNetContextMapper.MapToCommandMetadata(Context),
                async () =>
                {
                    var u = user == null ?
                        Context.User :
                        await user.GetTrackedUserAsync();

                    var rateLimitResult = await _rateLimiter.VerifyDailyLimitAsync(Context.User, "horoscope");
                    if (rateLimitResult != null)
                        return rateLimitResult;

                    var zodiac = await _zodiacSignRepository.GetZodiacForUserAsync(u);

                    if (zodiac == null)
                        return new EmbedResult(EmbedFactory.CreateError($"{u.Username}'s birthday is not set. They can use the `setbirthday` command to set it."));

                    var horoscopeResult = await _horoscopeClient.GetHoroscopeAsync(zodiac);

                    switch (horoscopeResult)
                    {
                        case Horoscope horoscope:
                            return new EmbedResult(new EmbedBuilder()
                                .WithUserAsAuthor(Context.User)
                                .WithColor(TaylorBotColors.SuccessColor)
                                .WithTitle($"{zodiac} - {DateTime.UtcNow.ToString("dddd MMMM dd, yyyy", TaylorBotCulture.Culture)}")
                                .WithDescription(horoscope.Text)
                            .Build());

                        case HoroscopeUnavailable:
                            return new EmbedResult(EmbedFactory.CreateError($"Sorry, GaneshaSpeaks does not have an horoscope for {zodiac} today. 😢"));

                        case GaneshaSpeaksGenericErrorResult error:
                            return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                                $"GaneshaSpeaks returned an error. {(error.Error != null ? $"({error.Error}) " : string.Empty)}😢",
                                "The site might be down. Try again later!"
                            })));

                        default: throw new NotImplementedException();
                    }
                }
            );

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }
    }
}
