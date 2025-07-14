using Discord;
using Humanizer;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Random;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Commands;

public class HeistPlaySlashCommand(
    TaskExceptionLogger taskExceptionLogger,
    IOptionsMonitor<HeistOptions> options,
    IRateLimiter rateLimiter,
    IHeistRepository heistRepository,
    IHeistStatsRepository heistStatsRepository,
    TaypointAmountParser amountParser,
    ICryptoSecureRandom cryptoSecureRandom,
    IPseudoRandom pseudoRandom,
    CommandMentioner mention,
    InGuildPrecondition.Factory inGuild) : ISlashCommand<HeistPlaySlashCommand.Options>
{
    public const string PrefixCommandName = "heist";

    public static string CommandName => "heist play";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ITaypointAmount amount);

    public Command Heist(RunContext context, ITaypointAmount? amount, string? amountString = null) => new(
        new(Info.Name, Aliases: [PrefixCommandName], IsSlashCommand: context.SlashCommand != null),
        async () =>
        {
            if (amountString != null)
            {
                var parsed = await amountParser.ParseStringAsync(context, amountString);
                if (!parsed)
                {
                    return new EmbedResult(EmbedFactory.CreateError($"`amount`: {parsed.Error.Message}"));
                }
                amount = parsed.Value;
            }
            ArgumentNullException.ThrowIfNull(amount);

            var guild = context.Guild;
            ArgumentNullException.ThrowIfNull(guild);
            var fetched = guild.Fetched;
            ArgumentNullException.ThrowIfNull(fetched);

            var author = context.User;
            ArgumentNullException.ThrowIfNull(author.MemberInfo);
            DiscordMember member = new(author, author.MemberInfo);

            var rateLimitResult = await rateLimiter.VerifyDailyLimitAsync(author, "heist");
            if (rateLimitResult != null)
                return rateLimitResult;

            var delay = options.CurrentValue.TimeSpanBeforeHeistStarts!.Value;

            var result = await heistRepository.EnterHeistAsync(member, amount, delay);
            switch (result)
            {
                case HeistCreated:
                    {
                        _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
                            async () =>
                            {
                                await Task.Delay(delay);
                                await EndHeistAsync(fetched, guild, context);
                            },
                            nameof(EndHeistAsync))
                        );

                        var embed = new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor).WithDescription(
                            $"""
                            Heist started by {author.Mention}! The more people, the higher the rewards! 🤑
                            To join, use {mention.SlashCommand("heist play", context)} and invest points into the heist! 🕵️‍
                            The heist begins in **{delay.Humanize()}**. ⏰
                            """);

                        return new EmbedResult(embed.Build());
                    }

                case HeistEntered:
                    {
                        var embed = new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor).WithDescription(
                            $"""
                            {author.Mention} joined the heist! 🕵️‍
                            Get more people to join and rob a bigger bank! 💵
                            """);

                        return new EmbedResult(embed.Build());
                    }

                case InvestmentUpdated:
                    {
                        var embed = new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor).WithDescription(
                            $"""
                            {author.Mention}'s investment for the heist has been updated! 🕵️‍
                            Get more people to join and rob a bigger bank! 💵
                            """);

                        return new EmbedResult(embed.Build());
                    }

                default: throw new NotImplementedException();
            }
        },
        Preconditions: [
            inGuild.Create(botMustBeInGuild: true),
        ]
    );

    private async Task EndHeistAsync(IGuild guild, CommandGuild commandGuild, RunContext context)
    {
        var channel = await guild.GetTextChannelAsync(context.Channel.Id);

        var heisters = await heistRepository.EndHeistAsync(commandGuild);
        var bank = GetBank(heisters.Count);

        var roll = cryptoSecureRandom.GetInt32(1, 100);
        var won = roll >= bank.minimumRollForSuccess;

        var results = won
            ? await heistStatsRepository.WinHeistAsync(heisters, bank.payoutMultiplier)
            : await heistStatsRepository.LoseHeistAsync(heisters);

        var randomPlayer = pseudoRandom.GetRandomElement(results);

        var description =
            $"""
            ### {(won ? "The heist was a success!" : "The heist was a failure!")}
            The **{results.Count}** person crew heads to the **{bank.bankName}**
            {(won ? $"All thanks to <@{randomPlayer.UserId}>, the heist went perfectly. 💯" : $"The cops busted the crew because {GetFailureReason(randomPlayer.UserId)}. 👮")}
            {string.Join('\n', results.Select(r =>
                $"<@{r.UserId}> Invested {"taypoint".ToQuantity(r.InvestedCount, TaylorBotFormats.BoldReadable)}{(won ? $", made a profit of {r.ProfitCount.ToString(TaylorBotFormats.BoldReadable)}" : "")}, now has {r.FinalCount.ToString(TaylorBotFormats.Readable)}. {(won ? "💰" : "💸")}"))}
            """;

        var embed = new EmbedBuilder()
            .WithUserAsAuthor(context.User)
            .WithColor(DiscordColor.FromHexString(won ? "#43b581" : "#f04747"))
            .WithDescription(description.Truncate(EmbedBuilder.MaxDescriptionLength));

        await channel.SendMessageAsync(embed: embed.Build());
    }

    private sealed record Bank(string bankName, ushort? maximumUserCount, ushort minimumRollForSuccess, string payoutMultiplier);

    private readonly Lazy<List<Bank>> banks = new(() =>
    {
        var encoded = options.CurrentValue.BanksJsonBase64;
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        return JsonSerializer.Deserialize<List<Bank>>(json) ?? throw new ArgumentNullException();
    });

    private Bank GetBank(int playerCount)
    {
        return banks.Value.First(b => b.maximumUserCount == null || playerCount <= b.maximumUserCount);
    }

    private static readonly string[] FailureReasons = [
        "{user}, the driver, never showed up to pick up the crew",
        "{user}, the driver, crashed the car on the way to the bank",
        "{user}, the hacker, was unable to hack into the surveillance security system",
        "{user}, the acrobat, was unable to dodge the vault's security lasers",
        "{user}, the pickpocket, wasn't able to steal the key from the security guard",
        "{user}, the pickpocket, took too long to crack the safe open",
        "{user} forgot all their equipment at home",
        "{user} got confused and actually went to open up a bank account",
        "{user} had last minute second thoughts and turned the entire crew in to the police",
        "{user} removed their gloves and left their DNA all over the vault room",
        "{user} got drunk to handle stress better but ended up getting easily disarmed by a civilian",
        "{user} was lighting a cigar in the vault to celebrate and accidentally burned the money",
        "{user} was secretly an undercover police officer",
        "the employees rang the alarm as soon as they saw {user} enter the bank",
        "{user} had the floor plans for the wrong bank"
    ];

    private string GetFailureReason(string userId) =>
        pseudoRandom.GetRandomElement(FailureReasons).Replace("{user}", $"<@{userId}>", StringComparison.InvariantCulture);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Heist(context, options.amount));
    }
}
