using Discord;
using Discord.Commands;
using Humanizer;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    public interface ICryptoSecureRandom
    {
        int GetRandomInt32(int fromInclusive, int toExclusive);
        T GetRandomElement<T>(IReadOnlyList<T> list);
    }

    public class CryptoSecureRandom : ICryptoSecureRandom
    {
        public int GetRandomInt32(int fromInclusive, int toExclusive) => RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);

        public T GetRandomElement<T>(IReadOnlyList<T> list) => list[RandomNumberGenerator.GetInt32(0, list.Count)];
    }

    [Name("Random 🎲")]
    public class RandomModule : TaylorBotModule
    {
        private readonly ICryptoSecureRandom _cryptoSecureRandom;

        public RandomModule(ICryptoSecureRandom cryptoSecureRandom)
        {
            _cryptoSecureRandom = cryptoSecureRandom;
        }

        [Command("dice")]
        [Summary("Rolls a dice with the specified amount of faces.")]
        public Task<RuntimeResult> DiceAsync(
            [Remainder]
            [Summary("How many faces should your dice have?")]
            PositiveInt32 faces
        )
        {
            var randomNumber = _cryptoSecureRandom.GetRandomInt32(0, faces.Parsed) + 1;

            return Task.FromResult<RuntimeResult>(new TaylorBotEmbedResult(new EmbedBuilder()
                .WithUserAsAuthor(Context.User)
                .WithColor(TaylorBotColors.SuccessColor)
                .WithTitle($"Rolling a dice with {"face".ToQuantity(faces.Parsed, TaylorBotFormats.Readable)} 🎲")
                .WithDescription($"You rolled {randomNumber.ToString(TaylorBotFormats.BoldReadable)}!")
            .Build()));
        }

        [Command("choose")]
        [Alias("choice")]
        [Summary("Chooses a random option from a list.")]
        public Task<RuntimeResult> ChooseAsync(
            [Remainder]
            [Summary("What are the options (comma separated) to choose from?")]
            string options
        )
        {
            var parsedOptions = options.Split(',').Select(o => o.Trim()).Where(o => !string.IsNullOrWhiteSpace(o)).ToList();

            var randomOption = _cryptoSecureRandom.GetRandomElement(parsedOptions);

            return Task.FromResult<RuntimeResult>(new TaylorBotEmbedResult(new EmbedBuilder()
                .WithUserAsAuthor(Context.User)
                .WithColor(TaylorBotColors.SuccessColor)
                .WithTitle("I choose:")
                .WithDescription(randomOption)
            .Build()));
        }
    }
}
