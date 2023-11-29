using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.WolframAlpha.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.WolframAlpha.Commands;

public class WolframAlphaSlashCommand : ISlashCommand<WolframAlphaSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("wolframalpha");

    public record Options(ParsedString question);

    private readonly IRateLimiter _rateLimiter;
    private readonly IWolframAlphaClient _wolframAlphaClient;

    public WolframAlphaSlashCommand(IRateLimiter rateLimiter, IWolframAlphaClient wolframAlphaClient)
    {
        _rateLimiter = rateLimiter;
        _wolframAlphaClient = wolframAlphaClient;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var rateLimitResult = await _rateLimiter.VerifyDailyLimitAsync(context.User, "wolframalpha-query");
                if (rateLimitResult != null)
                    return rateLimitResult;

                var result = await _wolframAlphaClient.QueryAsync(options.question.Value);

                switch (result)
                {
                    case WolframAlphaResult queryResult:
                        return new EmbedResult(new EmbedBuilder()
                            .WithColor(255, 125, 0)
                            .WithTitle(queryResult.InputPod.PlainText)
                            .WithImageUrl(queryResult.OutputPod.ImageUrl)
                            .WithFooter($"Wolfram|Alpha", iconUrl: "https://i.imgur.com/aHl1jlS.png")
                        .Build());

                    case WolframAlphaFailed _:
                        return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                            "WolframAlpha did not understand what you meant by:",
                            $"> {options.question.Value}",
                            "Try rephrasing your question! 😕"
                        })));

                    case GenericWolframAlphaError _:
                        return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                            "WolframAlpha returned an unexpected error. 😢",
                            "The site might be down. Try again later!"
                        })));

                    default:
                        throw new InvalidOperationException(result.GetType().Name);
                };
            }
        ));
    }
}
