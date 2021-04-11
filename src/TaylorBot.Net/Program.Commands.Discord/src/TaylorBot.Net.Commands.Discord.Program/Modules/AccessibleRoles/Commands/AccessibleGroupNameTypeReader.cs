using Discord.Commands;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Domain;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Commands
{
    public class AccessibleGroupNameTypeReader : TypeReader, ITaylorBotTypeReader
    {
        public Type ArgumentType => typeof(AccessibleGroupName);

        private const int MAX_LENGTH = 256;

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var trimmed = input.Trim();

            if (trimmed.Length > MAX_LENGTH)
            {
                return Task.FromResult(TypeReaderResult.FromError(
                    CommandError.ParseFailed, $"Group name must be equal to or less than {MAX_LENGTH} characters."
                ));
            }
            else
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(
                    new AccessibleGroupName(trimmed.ToLowerInvariant())
                ));
            }
        }
    }
}
