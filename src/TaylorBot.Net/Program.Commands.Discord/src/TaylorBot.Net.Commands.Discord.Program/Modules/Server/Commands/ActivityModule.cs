using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

[Name("Activity 📚")]
public class ActivityModule(ICommandRunner commandRunner, ServerMessagesSlashCommand serverMessagesCommand, ServerMinutesSlashCommand serverMinutesCommand) : TaylorBotModule
{
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
        var result = await commandRunner.RunSlashCommandAsync(
            serverMessagesCommand.Messages(new((IGuildUser)u)),
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
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </server leaderboard:1137547317549998130> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

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
        var result = await commandRunner.RunSlashCommandAsync(
            serverMinutesCommand.Minutes(new((IGuildUser)u)),
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
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </server leaderboard:1137547317549998130> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
