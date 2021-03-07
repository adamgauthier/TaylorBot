using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Logs.Domain;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [Name("Log 🪵")]
    [Group("log")]
    public class LogModule : ModuleBase
    {
        [Name("Deleted Logs 🗑")]
        [Group("deleted")]
        public class DeletedModule : TaylorBotModule
        {
            private readonly IDeletedLogChannelRepository _deletedLogChannelRepository;

            public DeletedModule(IDeletedLogChannelRepository deletedLogChannelRepository)
            {
                _deletedLogChannelRepository = deletedLogChannelRepository;
            }

            [RequireInGuild]
            [RequirePlus(PlusRequirement.PlusGuild)]
            [RequireUserPermissionOrOwner(GuildPermission.ManageGuild)]
            [Priority(-1)]
            [Command]
            [Summary("Directs TaylorBot to log deleted messages in a specific channel.")]
            public async Task<RuntimeResult> AddAsync(
                [Summary("What channel would you like deleted messages to be logged in?")]
                [Remainder]
                IChannelArgument<ITextChannel>? channel = null
            )
            {
                var textChannel = channel == null ? (ITextChannel)Context.Channel : channel.Channel;

                await _deletedLogChannelRepository.AddOrUpdateDeletedLogAsync(textChannel);

                return new TaylorBotEmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithUserAsAuthor(Context.User)
                    .WithDescription(string.Join('\n', new[] {
                        $"Ok, I will now log deleted messages in {textChannel.Mention}. Please wait up to 5 minutes for changes to take effect. ⌚",
                        $"Use `{Context.CommandPrefix}log deleted stop` to undo this action."
                    }))
                .Build());
            }

            [RequireInGuild]
            [RequireUserPermissionOrOwner(GuildPermission.ManageGuild)]
            [Command("stop")]
            [Summary("Directs TaylorBot to stop logging deleted messages in this server.")]
            public async Task<RuntimeResult> RemoveAsync()
            {
                await _deletedLogChannelRepository.RemoveDeletedLogAsync(Context.Guild);

                return new TaylorBotEmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithUserAsAuthor(Context.User)
                    .WithDescription(string.Join('\n', new[] {
                        "Ok, I will stop logging deleted messages in this server. Please wait up to 5 minutes for changes to take effect. ⌚",
                        $"Use `{Context.CommandPrefix}log deleted` to log deleted messages in a specific channel."
                    }))
                .Build());
            }
        }

        [Name("Member Logs 🧍")]
        [Group("member")]
        public class MemberModule : TaylorBotModule
        {
            private readonly IMemberLogChannelRepository _memberLogChannelRepository;

            public MemberModule(IMemberLogChannelRepository memberLogChannelRepository)
            {
                _memberLogChannelRepository = memberLogChannelRepository;
            }

            [RequireInGuild]
            [RequirePlus(PlusRequirement.PlusGuild)]
            [RequireUserPermissionOrOwner(GuildPermission.ManageGuild)]
            [Priority(-1)]
            [Command]
            [Summary("Directs TaylorBot to log member events in a specific channel.")]
            public async Task<RuntimeResult> AddAsync(
                [Summary("What channel would you like member events to be logged in?")]
                [Remainder]
                IChannelArgument<ITextChannel>? channel = null
            )
            {
                var textChannel = channel == null ? (ITextChannel)Context.Channel : channel.Channel;

                await _memberLogChannelRepository.AddOrUpdateMemberLogAsync(textChannel);

                return new TaylorBotEmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithUserAsAuthor(Context.User)
                    .WithDescription(string.Join('\n', new[] {
                        $"Ok, I will now log member joins, leaves and bans in {textChannel.Mention}. 😊",
                        $"Use `{Context.CommandPrefix}log member stop` to undo this action."
                    }))
                .Build());
            }

            [RequireInGuild]
            [RequireUserPermissionOrOwner(GuildPermission.ManageGuild)]
            [Command("stop")]
            [Summary("Directs TaylorBot to stop logging member events in this server.")]
            public async Task<RuntimeResult> RemoveAsync()
            {
                await _memberLogChannelRepository.RemoveMemberLogAsync(Context.Guild);

                return new TaylorBotEmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithUserAsAuthor(Context.User)
                    .WithDescription(string.Join('\n', new[] {
                        "Ok, I will stop logging member events in this server. 😊",
                        $"Use `{Context.CommandPrefix}log member` to log member events in a specific channel."
                    }))
                .Build());
            }
        }
    }
}
