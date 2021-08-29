using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Commands
{
    [Name("Log 🪵")]
    [Group("log")]
    public class LogModule : ModuleBase
    {
        [Name("Deleted Logs 🗑")]
        [Group("deleted")]
        public class DeletedModule : TaylorBotModule
        {
            private readonly ICommandRunner _commandRunner;

            public DeletedModule(ICommandRunner commandRunner)
            {
                _commandRunner = commandRunner;
            }

            [Priority(-1)]
            [Command]
            [Summary("This command has moved to a slash command. Use `/monitor deleted set` instead!")]
            public async Task<RuntimeResult> AddAsync(
                [Summary("What channel would you like deleted messages to be logged in?")]
                [Remainder]
                IChannelArgument<ITextChannel>? channel = null
            )
            {
                var command = new Command(
                    DiscordNetContextMapper.MapToCommandMetadata(Context),
                    () =>
                    {
                        return new(new EmbedResult(new EmbedBuilder()
                            .WithColor(TaylorBotColors.ErrorColor)
                            .WithUserAsAuthor(Context.User)
                            .WithDescription("This command has moved to a slash command. Use `/monitor deleted set` instead!")
                        .Build()));
                    }
                );

                var context = DiscordNetContextMapper.MapToRunContext(Context);
                var result = await _commandRunner.RunAsync(command, context);

                return new TaylorBotResult(result, context);
            }

            [Command("stop")]
            [Summary("This command has moved to a slash command. Use `/monitor deleted stop` instead!")]
            public async Task<RuntimeResult> RemoveAsync()
            {
                var command = new Command(
                    DiscordNetContextMapper.MapToCommandMetadata(Context),
                    () =>
                    {
                        return new(new EmbedResult(new EmbedBuilder()
                            .WithColor(TaylorBotColors.ErrorColor)
                            .WithUserAsAuthor(Context.User)
                            .WithDescription("This command has moved to a slash command. Use `/monitor deleted stop` instead!")
                        .Build()));
                    }
                );

                var context = DiscordNetContextMapper.MapToRunContext(Context);
                var result = await _commandRunner.RunAsync(command, context);

                return new TaylorBotResult(result, context);
            }
        }

        [Name("Member Logs 🧍")]
        [Group("member")]
        public class MemberModule : TaylorBotModule
        {
            private readonly ICommandRunner _commandRunner;
            private readonly IPlusRepository _plusRepository;
            private readonly IMemberLogChannelRepository _memberLogChannelRepository;

            public MemberModule(ICommandRunner commandRunner, IPlusRepository plusRepository, IMemberLogChannelRepository memberLogChannelRepository)
            {
                _commandRunner = commandRunner;
                _plusRepository = plusRepository;
                _memberLogChannelRepository = memberLogChannelRepository;
            }

            [Priority(-1)]
            [Command]
            [Summary("Directs TaylorBot to log member events in a specific channel.")]
            public async Task<RuntimeResult> AddAsync(
                [Summary("What channel would you like member events to be logged in?")]
                [Remainder]
                IChannelArgument<ITextChannel>? channel = null
            )
            {
                var command = new Command(
                    DiscordNetContextMapper.MapToCommandMetadata(Context),
                    async () =>
                    {
                        var textChannel = channel == null ? (ITextChannel)Context.Channel : channel.Channel;

                        if (textChannel is IThreadChannel thread)
                        {
                            return new EmbedResult(EmbedFactory.CreateError($"Channel '{thread.Name}' is a thread! Please use another text channel."));
                        }

                        await _memberLogChannelRepository.AddOrUpdateMemberLogAsync(textChannel);

                        return new EmbedResult(new EmbedBuilder()
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithUserAsAuthor(Context.User)
                            .WithDescription(string.Join('\n', new[] {
                                $"Ok, I will now log member joins, leaves and bans in {textChannel.Mention}. 😊",
                                $"Use `{Context.CommandPrefix}log member stop` to undo this action."
                            }))
                        .Build());
                    },
                    Preconditions: new ICommandPrecondition[] {
                        new InGuildPrecondition(),
                        new PlusPrecondition(_plusRepository, PlusRequirement.PlusGuild),
                        new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
                    }
                );

                var context = DiscordNetContextMapper.MapToRunContext(Context);
                var result = await _commandRunner.RunAsync(command, context);

                return new TaylorBotResult(result, context);
            }

            [Command("stop")]
            [Summary("Directs TaylorBot to stop logging member events in this server.")]
            public async Task<RuntimeResult> RemoveAsync()
            {
                var command = new Command(
                    DiscordNetContextMapper.MapToCommandMetadata(Context),
                    async () =>
                    {
                        await _memberLogChannelRepository.RemoveMemberLogAsync(Context.Guild);

                        return new EmbedResult(new EmbedBuilder()
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithUserAsAuthor(Context.User)
                            .WithDescription(string.Join('\n', new[] {
                                "Ok, I will stop logging member events in this server. 😊",
                                $"Use `{Context.CommandPrefix}log member` to log member events in a specific channel."
                            }))
                        .Build());
                    },
                    Preconditions: new ICommandPrecondition[] {
                        new InGuildPrecondition(),
                        new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
                    }
                );

                var context = DiscordNetContextMapper.MapToRunContext(Context);
                var result = await _commandRunner.RunAsync(command, context);

                return new TaylorBotResult(result, context);
            }
        }
    }
}
