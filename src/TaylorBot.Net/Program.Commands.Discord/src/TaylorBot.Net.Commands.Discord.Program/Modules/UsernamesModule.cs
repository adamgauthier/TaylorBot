using Discord;
using Discord.Commands;
using Microsoft.FSharp.Collections;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.UsernameHistory.Domain;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [Name("Usernames 🏷️")]
    [Group("usernames")]
    [Alias("names")]
    public class UsernamesModule : TaylorBotModule
    {
        private readonly IUsernameHistoryRepository _usernameHistoryRepository;

        public UsernamesModule(IUsernameHistoryRepository usernameHistoryRepository)
        {
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
            IUser u = user == null ?
                Context.User :
                await user.GetTrackedUserAsync();

            EmbedBuilder BuildBaseEmbed() =>
                new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor).WithUserAsAuthor(u);

            if (await _usernameHistoryRepository.IsUsernameHistoryHiddenFor(u))
            {
                return new TaylorBotEmbedResult(BuildBaseEmbed()
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
                    SeqModule.ChunkBySize(15, usernamesAsLines)
                    .Select(lines => string.Join('\n', lines))
                    .ToList();

                return new TaylorBotPageMessageResult(new PageMessage(
                    new EmbedDescriptionPageMessageRenderer(pages, BuildBaseEmbed)
                ));
            }
        }

        [Command("private")]
        [Summary("Hides your username history from other users.")]
        public async Task<RuntimeResult> PrivateAsync()
        {
            await _usernameHistoryRepository.HideUsernameHistoryFor(Context.User);

            return new TaylorBotEmbedResult(new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithUserAsAuthor(Context.User)
                .WithDescription(string.Join('\n', new[] {
                    $"Your username history is now private, it **can't** be viewed with `{Context.CommandPrefix}usernames`.",
                    $"Use `{Context.CommandPrefix}usernames public` to make it public."
                }))
            .Build());
        }

        [Command("public")]
        [Summary("Makes your username history public for other users to see.")]
        public async Task<RuntimeResult> PublicAsync()
        {
            await _usernameHistoryRepository.UnhideUsernameHistoryFor(Context.User);

            return new TaylorBotEmbedResult(new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithUserAsAuthor(Context.User)
                .WithDescription(string.Join('\n', new[] {
                    $"Your username history is now public, it **can** be viewed with `{Context.CommandPrefix}usernames`.",
                    $"Use `{Context.CommandPrefix}usernames private` to make it private."
                }))
            .Build());
        }
    }
}
