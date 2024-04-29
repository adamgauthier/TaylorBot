using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;

public class BirthdayHoroscopeSlashCommand(IRateLimiter rateLimiter, IZodiacSignRepository zodiacSignRepository, IHoroscopeClient horoscopeClient) : ISlashCommand<BirthdayHoroscopeSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("birthday horoscope");

    public record Options(ParsedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var user = options.user.User;

                var rateLimitResult = await rateLimiter.VerifyDailyLimitAsync(context.User, "horoscope");
                if (rateLimitResult != null)
                    return rateLimitResult;

                var zodiac = await zodiacSignRepository.GetZodiacForUserAsync(user);

                if (zodiac == null)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        {user.Mention}'s birthday is not set. 🚫
                        They need to use {context.MentionCommand("birthday set")} to set it first.
                        """));
                }

                var horoscopeResult = await horoscopeClient.GetHoroscopeAsync(zodiac);

                switch (horoscopeResult)
                {
                    case Horoscope horoscope:
                        return new EmbedResult(new EmbedBuilder()
                            .WithUserAsAuthor(user)
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithTitle($"{zodiac} - {DateTime.UtcNow.ToString("dddd MMMM dd, yyyy", TaylorBotCulture.Culture)}")
                            .WithDescription(horoscope.Text)
                        .Build());

                    case HoroscopeUnavailable:
                        return new EmbedResult(EmbedFactory.CreateError($"Sorry, GaneshaSpeaks does not have an horoscope for {zodiac} today. 😢"));

                    case GaneshaSpeaksGenericErrorResult error:
                        return new EmbedResult(EmbedFactory.CreateError(
                            $"""
                            GaneshaSpeaks returned an error. {(error.Error != null ? $"({error.Error}) " : string.Empty)}😢
                            The site might be down. Try again later!
                            """));

                    default: throw new NotImplementedException();
                }
            }
        ));
    }
}
