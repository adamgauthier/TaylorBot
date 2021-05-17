﻿using Discord;
using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers
{
    public record ParsedMember(IGuildUser Member);

    public class MemberParser : IOptionParser<ParsedMember>
    {
        private readonly ITaylorBotClient _taylorBotClient;

        public MemberParser(ITaylorBotClient taylorBotClient)
        {
            _taylorBotClient = taylorBotClient;
        }

        public async ValueTask<Result<ParsedMember, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            if (!optionValue.HasValue)
            {
                return Error(new ParsingFailed("Member option is required."));
            }
            if (context.Guild == null)
            {
                return Error(new ParsingFailed("Member option can only be used in a server."));
            }

            var member = await _taylorBotClient.ResolveGuildUserAsync(context.Guild, new(optionValue.Value.GetString()!));

            if (member == null)
            {
                return Error(new ParsingFailed($"Did not find member in the current server ({context.Guild.Name})."));
            }

            return new ParsedMember(member);
        }
    }
}
