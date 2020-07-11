using Discord;
using System.Linq;
using System.Threading.Tasks;

namespace TaylorBot.Net.QuickStart.Domain
{
    public class QuickStartChannelFinder
    {
        public async ValueTask<U?> FindQuickStartChannelAsync<T, U>(T guild)
            where T : IGuild
            where U : class, ITextChannel
        {
            var textChannels = (await guild.GetTextChannelsAsync()).Cast<U>();
            var currentUser = await guild.GetCurrentUserAsync();

            var availableChannels = textChannels.Where(channel =>
               IsEveryoneAllowedToSendMessages(guild, channel) &&
               currentUser.GetPermissions(channel).Has(ChannelPermission.SendMessages)
            ).ToList();

            if (!availableChannels.Any())
                return null;

            foreach (var name in new[] { "general", "main" })
            {
                var namedGeneral = availableChannels.FirstOrDefault(channel => channel.Name.Contains(name));

                if (namedGeneral != null)
                    return namedGeneral;
            }

            return availableChannels.First();
        }

        private static bool IsEveryoneAllowedToSendMessages(IGuild guild, ITextChannel channel)
        {
            var overwrite = channel.GetPermissionOverwrite(guild.EveryoneRole);
            return !overwrite.HasValue || overwrite.Value.SendMessages != PermValue.Deny;
        }
    }
}
