using Discord.Commands;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.LastFm.Domain;

namespace TaylorBot.Net.Commands.Discord.Program.LastFm.TypeReaders
{
    public class LastFmUsernameTypeReader : TypeReader, ITaylorBotTypeReader
    {
        public Type ArgumentType => typeof(LastFmUsername);

        private static readonly Regex UsernameRegex = new Regex(@"^[a-z0-9_-]{1,15}$", RegexOptions.IgnoreCase);
        private static readonly Regex LinkRegex = new Regex(@"^\/user\/([a-z0-9_-]{1,15})(\/.*)?$", RegexOptions.IgnoreCase);

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var trimmed = input.Trim();

            var match = UsernameRegex.Match(trimmed);

            if (match.Success)
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(
                    new LastFmUsername(match.Value)
                ));
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
                            return Task.FromResult(TypeReaderResult.FromSuccess(
                                new LastFmUsername(matches.Groups[1].Value)
                            ));
                        }
                    }
                }
                catch (UriFormatException)
                {
                    // Continue on error, it's not a URL
                }

                return Task.FromResult(TypeReaderResult.FromError(
                    CommandError.ParseFailed, $"Could not parse '{input}' into a valid Last.fm username."
                ));
            }
        }
    }
}
