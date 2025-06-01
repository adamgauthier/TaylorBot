using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Help.Domain;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using static TaylorBot.Net.Commands.PostExecution.InteractionResponse;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Help.Commands;

public enum HelpCategory
{
    Moderation,
    Fun,
    Utility,
    Music,
    Games,
    Economy,
    Settings,
    Information,
    Other
}

public class HelpSlashCommand : ISlashCommand<NoOptions>
{
    public static string CommandName => "help";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public IList<ICommandPrecondition> BuildPreconditions() => [];

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions _)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var embed = EmbedFactory.CreateSuccess(
                    $"""
                    ## TaylorBot Help Center 🎓
                    Here are the main categories of commands available:

                    • **Moderation** - Server management and moderation tools
                    • **Fun** - Entertainment and interactive commands
                    • **Utility** - Useful tools and utilities
                    • **Music** - Music playback and control
                    • **Games** - Fun games and activities
                    • **Economy** - Currency and economy system
                    • **Settings** - Bot configuration and preferences
                    • **Information** - Server and user information
                    • **Other** - Miscellaneous commands

                    Click a button below to see detailed information about each category! 🔍
                    """);

                var components = new List<InteractionComponent>
                {
                    CreateCategoryButtons()
                };

                return new MessageResult(new(new(embed), components));
            },
            Preconditions: BuildPreconditions()
        ));
    }

    private static InteractionComponent CreateCategoryButtons()
    {
        var buttons = new List<InteractionComponent>();

        foreach (HelpCategory category in Enum.GetValues(typeof(HelpCategory)))
        {
            buttons.Add(InteractionComponent.CreateButton(
                style: InteractionButtonStyle.Secondary,
                custom_id: InteractionCustomId.Create(CustomIdNames.HelpCategory, [new("category", category.ToString())]).RawId,
                label: category.ToString(),
                emoji: GetCategoryEmoji(category)));
        }

        return InteractionComponent.CreateActionRow(buttons);
    }

    private static string GetCategoryEmoji(HelpCategory category) => category switch
    {
        HelpCategory.Moderation => "🛡️",
        HelpCategory.Fun => "🎮",
        HelpCategory.Utility => "🛠️",
        HelpCategory.Music => "🎵",
        HelpCategory.Games => "🎲",
        HelpCategory.Economy => "💰",
        HelpCategory.Settings => "⚙️",
        HelpCategory.Information => "ℹ️",
        HelpCategory.Other => "📦",
        _ => "❓"
    };
}

public class HelpCategoryHandler : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.HelpCategory;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var category = Enum.Parse<HelpCategory>(button.CustomId.ParsedData["category"]);

        var embed = GetCategoryEmbed(category);
        var components = new List<InteractionComponent>
        {
            InteractionComponent.CreateActionRow(InteractionComponent.CreateButton(
                style: InteractionButtonStyle.Secondary,
                custom_id: InteractionCustomId.Create(CustomIdNames.HelpBack).RawId,
                label: "Back to Help Homepage",
                emoji: new("⬅️")))
        };

        await context.InteractionResponseClient.EditOriginalResponseAsync(button.Interaction, new MessageResponse(new([embed]), components));
    }

    private static Embed GetCategoryEmbed(HelpCategory category)
    {
        return category switch
        {
            HelpCategory.Moderation => EmbedFactory.CreateSuccess(
                $"""
                ## Moderation Commands 🛡️
                • `/ban` - Ban a user from the server
                • `/kick` - Kick a user from the server
                • `/mute` - Mute a user temporarily
                • `/clear` - Clear messages in a channel
                • `/warn` - Warn a user
                • `/modmail` - Configure modmail system
                """),
            HelpCategory.Fun => EmbedFactory.CreateSuccess(
                $"""
                ## Fun Commands 🎮
                • `/8ball` - Ask the magic 8ball
                • `/meme` - Get a random meme
                • `/joke` - Get a random joke
                • `/roll` - Roll some dice
                • `/flip` - Flip a coin
                """),
            HelpCategory.Utility => EmbedFactory.CreateSuccess(
                $"""
                ## Utility Commands 🛠️
                • `/avatar` - Get user avatars
                • `/serverinfo` - Get server information
                • `/userinfo` - Get user information
                • `/remind` - Set a reminder
                • `/poll` - Create a poll
                """),
            HelpCategory.Music => EmbedFactory.CreateSuccess(
                $"""
                ## Music Commands 🎵
                • `/play` - Play a song
                • `/skip` - Skip current song
                • `/queue` - View music queue
                • `/pause` - Pause playback
                • `/resume` - Resume playback
                """),
            HelpCategory.Games => EmbedFactory.CreateSuccess(
                $"""
                ## Games Commands 🎲
                • `/trivia` - Play trivia
                • `/hangman` - Play hangman
                • `/tictactoe` - Play tic-tac-toe
                • `/blackjack` - Play blackjack
                """),
            HelpCategory.Economy => EmbedFactory.CreateSuccess(
                $"""
                ## Economy Commands 💰
                • `/balance` - Check your balance
                • `/daily` - Get daily coins
                • `/work` - Work for coins
                • `/shop` - View the shop
                """),
            HelpCategory.Settings => EmbedFactory.CreateSuccess(
                $"""
                ## Settings Commands ⚙️
                • `/prefix` - Change bot prefix
                • `/language` - Change bot language
                • `/welcome` - Configure welcome messages
                • `/autorole` - Configure auto roles
                """),
            HelpCategory.Information => EmbedFactory.CreateSuccess(
                $"""
                ## Information Commands ℹ️
                • `/help` - Show this help menu
                • `/ping` - Check bot latency
                • `/stats` - View bot statistics
                • `/invite` - Get bot invite link
                """),
            HelpCategory.Other => EmbedFactory.CreateSuccess(
                $"""
                ## Other Commands 📦
                • `/feedback` - Send feedback
                • `/bug` - Report a bug
                • `/suggest` - Make a suggestion
                """),
            _ => EmbedFactory.CreateError("Invalid category selected!")
        };
    }
}

public class HelpBackHandler : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.HelpBack;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var helpCommand = new HelpSlashCommand();
        var command = await helpCommand.GetCommandAsync(context, new NoOptions());
        await command.ExecuteAsync();
    }
}
