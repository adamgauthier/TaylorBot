using Discord;
using Discord.Commands;
using Humanizer;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [Name("Random 🎲")]
    public class RandomModule : TaylorBotModule
    {
        [Command("dice")]
        [Summary("Rolls a dice with the specified amount of faces.")]
        public Task<RuntimeResult> DiceAsync(PositiveInt32 faces)
        {
            var randomNumber = RandomNumberGenerator.GetInt32(0, faces.Parsed) + 1;

            return Task.FromResult<RuntimeResult>(new TaylorBotEmbedResult(new EmbedBuilder()
                .WithUserAsAuthor(Context.User)
                .WithColor(TaylorBotColors.SuccessColor)
                .WithTitle($"Rolling a dice with {"face".ToQuantity(faces.Parsed, TaylorBotFormats.Readable)} 🎲")
                .WithDescription($"You rolled {randomNumber.ToString(TaylorBotFormats.BoldReadable)}!")
            .Build()));
        }
    }
}
