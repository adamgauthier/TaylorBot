using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Valentines.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Valentines.Commands
{
    public class LoveSpreadSlashCommand : ISlashCommand<LoveSpreadSlashCommand.Options>
    {
        public SlashCommandInfo Info => new("love spread");

        public record Options(ParsedMemberNotAuthor user);

        private readonly IValentinesRepository _valentinesRepository;
        private readonly ITaylorBotClient _taylorBotClient;

        public LoveSpreadSlashCommand(IValentinesRepository valentinesRepository, ITaylorBotClient taylorBotClient)
        {
            _valentinesRepository = valentinesRepository;
            _taylorBotClient = taylorBotClient;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new Command(
                new(Info.Name),
                async () =>
                {
                    var config = await _valentinesRepository.GetConfigurationAsync();

                    var author = (IGuildUser)context.User;

                    if (!author.RoleIds.Any(i => i == config.SpreadLoveRoleId.Id))
                    {
                        return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                            $"You don't have the {MentionUtils.MentionRole(config.SpreadLoveRoleId.Id)} role. 😭",
                            $"Once someone with the role spreads it to you, you will be able to use this command! 💖"
                        })));
                    }

                    var member = options.user.Member;

                    if (member.RoleIds.Any(i => i == config.SpreadLoveRoleId.Id))
                    {
                        return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                            $"{member.Mention} already has the {MentionUtils.MentionRole(config.SpreadLoveRoleId.Id)} role. 🥺",
                            $"Please spread love to another bestie who doesn't have it already! 💖"
                        })));
                    }

                    var authorObtained = await _valentinesRepository.GetRoleObtainedByUserAsync(author);
                    if (authorObtained == null)
                    {
                        return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                            $"Oops, your {MentionUtils.MentionRole(config.SpreadLoveRoleId.Id)} role has not been obtained legitimately. 😭",
                            $"Please make sure you get it from someone spreading love to you! 💖"
                        })));
                    }

                    var memberObtained = await _valentinesRepository.GetRoleObtainedByUserAsync(member);
                    if (memberObtained != null)
                    {
                        return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                            $"Oops, {member.Mention} has already been given {MentionUtils.MentionRole(config.SpreadLoveRoleId.Id)} role before. 😭",
                            $"Did someone remove the role manually? Ask a mod to give it back!"
                        })));
                    }

                    var authorCanSpreadAt = authorObtained.AcquiredAt + config.IncubationPeriod;
                    if (DateTimeOffset.UtcNow < authorCanSpreadAt)
                    {
                        return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                            $"You must wait a little more to be able to spread love to your besties. 🥺",
                            $"You will be able to spread love <t:{authorCanSpreadAt.ToUnixTimeSeconds()}:R>! 💖"
                        })));
                    }

                    var given = await _valentinesRepository.GetRoleObtainedFromUserAsync(author);
                    if (!author.RoleIds.Intersect(config.BypassSpreadLimitRoleIds.Select(i => i.Id)).Any())
                    {
                        if (given.Count >= config.SpreadLimit)
                        {
                            return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                                $"It looks like you already spread love to the maximum amount of besties (**{config.SpreadLimit}**). 🥺",
                                $"Thank you, sharing is caring! 💖"
                            })));
                        }
                    }

                    var acquiredAt = await _valentinesRepository.SpreadRoleAsync(author, member);
                    var memberCanSpreadAt = acquiredAt + config.IncubationPeriod;
                    var canStillGiveTo = config.SpreadLimit - (given.Count + 1);

                    await member.AddRoleAsync(config.SpreadLoveRoleId.Id);

                    var lounge = (ITextChannel)await _taylorBotClient.ResolveRequiredChannelAsync(config.LoungeChannelId);
                    await lounge.SendMessageAsync(
                        $"Welcome to our newest lover {member.Mention}, thanks to {author.Mention}! 💖",
                        allowedMentions: new() { UserIds = new List<ulong> { member.Id } }
                    );

                    return new EmbedResult(EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                        $"Your love 💕 is being delivered to {member.Mention} 💖🥰",
                        $"They will be able to spread love <t:{memberCanSpreadAt.ToUnixTimeSeconds()}:R>",
                        canStillGiveTo > 0
                            ? $"You have **{config.SpreadLimit}** more valentines 💌 with love you can send! 💝"
                            : "You've reached the spreading love limit and can't spread to more people, thank you. 🙏"
                    })));
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                }
            ));
        }
    }
}
