using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.DiscordNet;
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
            [Summary("This command has moved to </monitor deleted set:887146682146488390>.")]
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
                            .WithDescription("This command has moved to </monitor deleted set:887146682146488390>.")
                        .Build()));
                    }
                );

                var context = DiscordNetContextMapper.MapToRunContext(Context);
                var result = await _commandRunner.RunAsync(command, context);

                return new TaylorBotResult(result, context);
            }

            [Command("stop")]
            [Summary("This command has moved to </monitor deleted stop:887146682146488390>.")]
            public async Task<RuntimeResult> RemoveAsync()
            {
                var command = new Command(
                    DiscordNetContextMapper.MapToCommandMetadata(Context),
                    () =>
                    {
                        return new(new EmbedResult(new EmbedBuilder()
                            .WithColor(TaylorBotColors.ErrorColor)
                            .WithUserAsAuthor(Context.User)
                            .WithDescription("This command has moved to </monitor deleted stop:887146682146488390>.")
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

            public MemberModule(ICommandRunner commandRunner)
            {
                _commandRunner = commandRunner;
            }

            [Priority(-1)]
            [Command]
            [Summary("This command has moved to </monitor members set:887146682146488390>.")]
            public async Task<RuntimeResult> AddAsync(
                [Summary("What channel would you like member events to be logged in?")]
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
                            .WithDescription("This command has moved to </monitor members set:887146682146488390>.")
                        .Build()));
                    }
                );

                var context = DiscordNetContextMapper.MapToRunContext(Context);
                var result = await _commandRunner.RunAsync(command, context);

                return new TaylorBotResult(result, context);
            }

            [Command("stop")]
            [Summary("This command has moved to </monitor members stop:887146682146488390>.")]
            public async Task<RuntimeResult> RemoveAsync()
            {
                var command = new Command(
                    DiscordNetContextMapper.MapToCommandMetadata(Context),
                    () =>
                    {
                        return new(new EmbedResult(new EmbedBuilder()
                            .WithColor(TaylorBotColors.ErrorColor)
                            .WithUserAsAuthor(Context.User)
                            .WithDescription("This command has moved to </monitor members stop:887146682146488390>.")
                        .Build()));
                    }
                );

                var context = DiscordNetContextMapper.MapToRunContext(Context);
                var result = await _commandRunner.RunAsync(command, context);

                return new TaylorBotResult(result, context);
            }
        }
    }
}
