using OperationResult;
using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Parsers;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands
{
    public class LastFmUsernameParser : IOptionParser<LastFmUsername>
    {
        private static readonly Regex UsernameRegex = new(@"^[a-z0-9_-]{1,15}$", RegexOptions.IgnoreCase);
        private static readonly Regex LinkRegex = new(@"^\/user\/([a-z0-9_-]{1,15})(\/.*)?$", RegexOptions.IgnoreCase);

        public static Result<LastFmUsername, ParsingFailed> Parse(string input)
        {
            var trimmed = input.Trim();

            var match = UsernameRegex.Match(trimmed);

            if (match.Success)
            {
                return Ok(
                    new LastFmUsername(match.Value)
                );
            }
            else
            {
                try
                {
                    var url = new Uri(trimmed);
                    if (url.Host == "www.last.fm")
                    {
                        var matches = LinkRegex.Match(url.AbsolutePath);
                        if (matches.Success)
                        {
                            return Ok(
                                new LastFmUsername(matches.Groups[1].Value)
                            );
                        }
                    }
                }
                catch (UriFormatException)
                {
                    // Continue on error, it's not a URL
                }

                return Error(new ParsingFailed($"Could not parse '{input}' into a valid Last.fm username."));
            }
        }

        public ValueTask<Result<LastFmUsername, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            if (!optionValue.HasValue)
            {
                return new(Error(new ParsingFailed("Username option is required.")));
            }

            var input = optionValue.Value.GetString()!;

            return new(Parse(input));
        }
    }
}
