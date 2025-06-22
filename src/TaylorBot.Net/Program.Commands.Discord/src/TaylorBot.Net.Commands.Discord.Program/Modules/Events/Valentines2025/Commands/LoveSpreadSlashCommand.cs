using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2025.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2025.Commands;

public class LoveSpreadSlashCommand(
    Lazy<ITaylorBotClient> client,
    IValentinesRepository valentinesRepository,
    InGuildPrecondition.Factory inGuild) : ISlashCommand<LoveSpreadSlashCommand.Options>
{
    public static string CommandName => "love spread";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedMemberNotAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var config = await valentinesRepository.GetConfigurationAsync();

                ArgumentNullException.ThrowIfNull(context.Guild);

                var author = context.FetchedUser != null
                    ? (IGuildUser)context.FetchedUser
                    : await client.Value.ResolveGuildUserAsync(context.Guild.Id, context.User.Id) ?? throw new NotImplementedException();

                if (!author.RoleIds.Any(i => i == config.SpreadLoveRoleId.Id))
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        You don't have the {MentionUtils.MentionRole(config.SpreadLoveRoleId.Id)} role 😭
                        Once someone with the role spreads it to you, you will be able to use this command! 💖
                        """));
                }

                var member = options.user.Member;

                if (member.Member.Roles.Any(i => i == config.SpreadLoveRoleId))
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        {member.User.Mention} already has the {MentionUtils.MentionRole(config.SpreadLoveRoleId.Id)} role 🥺
                        Please spread love to another bestie who doesn't have it already! 💖
                        """));
                }

                var authorObtained = await valentinesRepository.GetRoleObtainedByUserAsync(new(author));
                if (authorObtained == null)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        Oops, your {MentionUtils.MentionRole(config.SpreadLoveRoleId.Id)} role has not been obtained legitimately 😭
                        Please make sure you get it from someone spreading love to you! 💖
                        """));
                }

                var memberObtained = await valentinesRepository.GetRoleObtainedByUserAsync(member);
                if (memberObtained != null)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        Oops, {member.User.Mention} has already been given {MentionUtils.MentionRole(config.SpreadLoveRoleId.Id)} role before 😭
                        Did someone remove the role manually? Ask a mod to give it back!
                        """));
                }

                var authorCanSpreadAt = authorObtained.AcquiredAt + config.IncubationPeriod;
                if (DateTimeOffset.UtcNow < authorCanSpreadAt)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        You must wait a little more to be able to spread love to your besties 🥺
                        You will be able to spread love <t:{authorCanSpreadAt.ToUnixTimeSeconds()}:R>! 💖
                        """));
                }

                var given = await valentinesRepository.GetRoleObtainedFromUserAsync(new(author));
                if (!author.RoleIds.Intersect(config.BypassSpreadLimitRoleIds.Select(i => i.Id)).Any())
                {
                    if (given.Count >= config.SpreadLimit)
                    {
                        return new EmbedResult(EmbedFactory.CreateError(
                            $"""
                            It looks like you already spread love to the maximum amount of besties (**{config.SpreadLimit}**) 🥺
                            Thank you, sharing is caring! 💖
                            """));
                    }
                }

                var acquiredAt = await valentinesRepository.SpreadRoleAsync(new(author), member);
                var memberCanSpreadAt = acquiredAt + config.IncubationPeriod;
                var canStillGiveTo = config.SpreadLimit - (given.Count + 1);

                await client.Value.RestClient.AddRoleAsync(member.Member.GuildId, member.User.Id, config.SpreadLoveRoleId);

                var lounge = (ITextChannel)await client.Value.ResolveRequiredChannelAsync(config.LoungeChannelId);
                await lounge.SendMessageAsync(
                    $"Welcome to our newest lover {member.User.Mention}, thanks to {author.Mention}! 💖",
                    allowedMentions: new() { UserIds = [member.User.Id] }
                );

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Your love 💕 is being delivered to {member.User.Mention} 💖🥰
                    They will be able to spread love <t:{memberCanSpreadAt.ToUnixTimeSeconds()}:R> ✨
                    {(canStillGiveTo > 0
                        ? $"You can still spread love to {"more bestie".ToQuantity(canStillGiveTo, TaylorBotFormats.BoldReadable)}! 💝"
                        : $"You can't spread love anymore, but you can enter giveaways in {lounge.Mention} 🙏")}
                    """));
            },
            Preconditions: [
                inGuild.Create(botMustBeInGuild: true),
            ]
        ));
    }
}
