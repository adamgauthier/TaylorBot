using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Commands
{
    [Name("Usernames 🏷️")]
    [Group("usernames")]
    [Alias("names")]
    public class UsernamesModule : TaylorBotModule
    {
        private readonly ICommandRunner _commandRunner;
        private readonly IUsernameHistoryRepository _usernameHistoryRepository;

        public UsernamesModule(ICommandRunner commandRunner, IUsernameHistoryRepository usernameHistoryRepository)
        {
            _commandRunner = commandRunner;
            _usernameHistoryRepository = usernameHistoryRepository;
        }

        [Priority(-1)]
        [Command]
        [Summary("Gets a list of previous usernames for a user.")]
        public async Task<RuntimeResult> GetAsync(
            [Summary("What user would you like to see the username history of?")]
            [Remainder]
            IUserArgument<IUser>? user = null
        )
        {
            var command = new Command(DiscordNetContextMapper.MapToCommandMetadata(Context), async () =>
            {
                IUser u = user == null ?
                    Context.User :
                    await user.GetTrackedUserAsync();

                EmbedBuilder BuildBaseEmbed() =>
                    new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor).WithUserAsAuthor(u);

                if (await _usernameHistoryRepository.IsUsernameHistoryHiddenFor(u))
                {
                    return new EmbedResult(BuildBaseEmbed()
                        .WithDescription(string.Join('\n', new[] {
                            $"{u.Username}'s username history is private and can't be viewed.",
                            $"Use `{Context.CommandPrefix}usernames public` or `{Context.CommandPrefix}usernames private` to change your username history privacy setting."
                        }))
                    .Build());
                }
                else
                {
                    var usernames = await _usernameHistoryRepository.GetUsernameHistoryFor(u, 75);

                    var usernamesAsLines = usernames.Select(u => $"{u.ChangedAt:MMMM dd, yyyy}: {u.Username}");

                    var pages =
                        usernamesAsLines.Chunk(size: 15)
                        .Select(lines => string.Join('\n', lines))
                        .ToList();

                    return new PageMessageResult(new PageMessage(new(
                        new EmbedPageMessageRenderer(new DescriptionPageEditor(pages), BuildBaseEmbed)
                    )));
                }
            });

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }

        [Command("private")]
        [Summary("Hides your username history from other users.")]
        public async Task<RuntimeResult> PrivateAsync()
        {
            var command = new Command(DiscordNetContextMapper.MapToCommandMetadata(Context), async () =>
            {
                await _usernameHistoryRepository.HideUsernameHistoryFor(Context.User);

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithUserAsAuthor(Context.User)
                    .WithDescription(string.Join('\n', new[] {
                        $"Your username history is now private, it **can't** be viewed with `{Context.CommandPrefix}usernames`.",
                        $"Use `{Context.CommandPrefix}usernames public` to make it public."
                    }))
                .Build());
            });

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }

        [Command("public")]
        [Summary("Makes your username history public for other users to see.")]
        public async Task<RuntimeResult> PublicAsync()
        {
            var command = new Command(DiscordNetContextMapper.MapToCommandMetadata(Context), async () =>
            {
                await _usernameHistoryRepository.UnhideUsernameHistoryFor(Context.User);

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithUserAsAuthor(Context.User)
                    .WithDescription(string.Join('\n', new[] {
                        $"Your username history is now public, it **can** be viewed with `{Context.CommandPrefix}usernames`.",
                        $"Use `{Context.CommandPrefix}usernames private` to make it private."
                    }))
                .Build());
            });

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }
    }
}
