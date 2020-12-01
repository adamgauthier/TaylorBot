using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [Name("Command")]
    [Group("command")]
    [Alias("c")]
    public class CommandModule : TaylorBotModule
    {
        private static readonly string[] GuardedModuleNames = new[] { "framework", "command" };

        private readonly IDisabledCommandRepository _disabledCommandRepository;

        public CommandModule(IDisabledCommandRepository disabledCommandRepository)
        {
            _disabledCommandRepository = disabledCommandRepository;
        }

        [RequireTaylorBotOwner]
        [Command("enable-global")]
        [Summary("Enables a command globally.")]
        public async Task<RuntimeResult> EnableGlobalAsync(
            [Summary("What command would you like to enable?")]
            [Remainder]
            ICommandRepository.Command command
        )
        {
            await _disabledCommandRepository.EnableGloballyAsync(command.Name);

            return new TaylorBotEmbedResult(new EmbedBuilder()
                .WithUserAsAuthor(Context.User)
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription($"Command `{command.Name}` has been enabled globally.")
            .Build());
        }

        [RequireTaylorBotOwner]
        [Command("disable-global")]
        [Summary("Disables a command globally.")]
        public async Task<RuntimeResult> DisableGlobalAsync(
            [Summary("What command would you like to disable?")]
            ICommandRepository.Command command,
            [Remainder]
            [Summary("What message should be shown to someone using the disabled command?")]
            string message
        )
        {
            var embed = new EmbedBuilder().WithUserAsAuthor(Context.User);

            if (GuardedModuleNames.Contains(command.ModuleName.ToLowerInvariant()))
            {
                return new TaylorBotEmbedResult(embed
                    .WithColor(TaylorBotColors.ErrorColor)
                    .WithDescription($"Command `{command.Name}` can't be disabled because it's a framework command.")
                .Build());
            }

            var disabledMessage = await _disabledCommandRepository.DisableGloballyAsync(command.Name, message);

            return new TaylorBotEmbedResult(embed
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription($"Command `{command.Name}` has been disabled globally with message '{disabledMessage}'.")
            .Build());
        }
    }
}
