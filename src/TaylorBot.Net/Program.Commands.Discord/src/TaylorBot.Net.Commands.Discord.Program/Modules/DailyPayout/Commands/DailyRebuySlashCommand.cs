using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Commands;

public class DailyRebuySlashCommand(IDailyPayoutRepository dailyPayoutRepository, CommandMentioner mention) : ISlashCommand<NoOptions>
{
    public const int RebuyPricePerDay = 50;

    public static string CommandName => "daily rebuy";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var streakInfo = await dailyPayoutRepository.GetStreakInfoAsync(context.User);

                if (!streakInfo.HasValue)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        You've never claimed your daily reward! ❌
                        Use {mention.SlashCommand("daily claim", context)} to start!
                        """));
                }
                else if (streakInfo.Value.MaxStreak > streakInfo.Value.CurrentStreak)
                {
                    var cost = streakInfo.Value.MaxStreak * RebuyPricePerDay;

                    return MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(
                            $"""
                            Are you sure you want to buy back your daily streak of {streakInfo.Value.MaxStreak.ToString(TaylorBotFormats.BoldReadable)}?
                            This will cost you {"taypoint".ToQuantity(cost, TaylorBotFormats.BoldReadable)} 💰
                            """)),
                        InteractionCustomId.Create(DailyRebuyConfirmButtonHandler.CustomIdName, [new("rpd", $"{RebuyPricePerDay}")])
                    );
                }
                else
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        Your current daily streak is the highest it's ever been ({streakInfo.Value.CurrentStreak.ToString(TaylorBotFormats.BoldReadable)})! ⭐
                        There is nothing to buy back!
                        """));
                }
            }
        ));
    }
}

public class DailyRebuyConfirmButtonHandler(IInteractionResponseClient responseClient, IDailyPayoutRepository dailyPayoutRepository) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.DailyRebuyConfirm;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var parsedSuccess = int.TryParse(button.CustomId.ParsedData["rpd"], out var rebuyPricePerDay);
        ArgumentOutOfRangeException.ThrowIfNotEqual(parsedSuccess, true);

        if (rebuyPricePerDay != DailyRebuySlashCommand.RebuyPricePerDay)
        {
            await responseClient.EditOriginalResponseAsync(button.Interaction, InteractionMapper.ToInteractionEmbed(EmbedFactory.CreateError(
                $"""
                Oops, looks like the rebuy price has changed since you ran this command 😵
                Please run the command again 🔃
                """)));
            return;
        }

        var result = await dailyPayoutRepository.RebuyMaxStreakAsync(context.User, rebuyPricePerDay);
        switch (result)
        {
            case RebuyResult success:
                await responseClient.EditOriginalResponseAsync(button.Interaction, InteractionMapper.ToInteractionEmbed(EmbedFactory.CreateSuccess(
                    $"""
                    Successfully reset your daily streak back to {success.CurrentDailyStreak.ToString(TaylorBotFormats.BoldReadable)}! 👍
                    You now have {"taypoint".ToQuantity(success.TotalTaypointCount, TaylorBotFormats.BoldReadable)} 👛
                    """)));
                break;

            case RebuyFailedInsufficientFunds failed:
                await responseClient.EditOriginalResponseAsync(button.Interaction, InteractionMapper.ToInteractionEmbed(EmbedFactory.CreateError(
                    $"""
                    Could not rebuy your streak. You only have {"taypoint".ToQuantity(failed.TotalTaypointCount, TaylorBotFormats.BoldReadable)} 😕
                    You need {"taypoint".ToQuantity(failed.Cost, TaylorBotFormats.BoldReadable)} 💰
                    """)));
                break;

            case RebuyFailedStaleStreak _:
                await responseClient.EditOriginalResponseAsync(button.Interaction, InteractionMapper.ToInteractionEmbed(EmbedFactory.CreateError(
                    $"""
                    It seems like your streak has changed since you ran the command 🤔
                    Please run the command again 🔃
                    """)));
                break;

            default: throw new InvalidOperationException($"Unexpected type {result.GetType().FullName}");
        }
    }
}
