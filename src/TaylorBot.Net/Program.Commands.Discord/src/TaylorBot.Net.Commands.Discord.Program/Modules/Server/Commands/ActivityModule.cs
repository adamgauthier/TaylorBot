using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

[Name("Activity 📚")]
public class ActivityModule : TaylorBotModule
{
    private readonly ICommandRunner _commandRunner;
    private readonly ServerMessagesSlashCommand _serverMessagesCommand;
    private readonly ServerMinutesSlashCommand _serverMinutesCommand;

    public ActivityModule(ICommandRunner commandRunner, ServerMessagesSlashCommand serverMessagesCommand, ServerMinutesSlashCommand serverMinutesCommand)
    {
        _commandRunner = commandRunner;
        _serverMessagesCommand = serverMessagesCommand;
        _serverMinutesCommand = serverMinutesCommand;
    }

    [Command("messages")]
    [Summary("Show the message count of a server member")]
    public async Task<RuntimeResult> MessagesAsync(
        [Summary("What user would you like to see the message count for?")]
        [Remainder]
        IUserArgument<IGuildUser>? user = null
    )
    {
        var u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(
            _serverMessagesCommand.Messages(u, isLegacyCommand: true),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("rankmessages")]
    [Alias("rank messages")]
    [Summary("This command has been moved to </server leaderboard:1137547317549998130>. Please use it instead! 😊")]
    public async Task<RuntimeResult> RankMessagesAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </server leaderboard:1137547317549998130> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("minutes")]
    [Summary("Show the minute count of a server member")]
    public async Task<RuntimeResult> MinutesAsync(
        [Summary("What user would you like to see the minute count for?")]
        [Remainder]
        IUserArgument<IGuildUser>? user = null
    )
    {
        var u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(
            _serverMinutesCommand.Minutes(u, isLegacyCommand: true),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("rankminutes")]
    [Alias("rank minutes")]
    [Summary("This command has been moved to </server leaderboard:1137547317549998130>. Please use it instead! 😊")]
    public async Task<RuntimeResult> RankMinutesAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </server leaderboard:1137547317549998130> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
