using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Poll.Commands;

[Name("Poll ❓")]
public class PollModule(ICommandRunner commandRunner) : TaylorBotModule
{
    private readonly static List<Emoji> Choices = ["1️⃣", "2️⃣", "3️⃣", "4️⃣"];

    [Command("poll")]
    [Alias("reactpoll", "rpoll")]
    [Summary("Creates a quick poll for a few options with reactions!")]
    public async Task<RuntimeResult> ReactPollAsync(
        [Summary("What are the options (comma separated) for your poll?")]
        [Remainder]
        string options
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () =>
            {
                var allOptions = options.Split(',').Select(o => o.Trim()).ToArray();
                if (allOptions.Length == 1)
                {
                    return new(new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        You can't start a poll with only 1 option 😕
                        Make sure you specify multiple options, for example:
                        > {Context.CommandPrefix}poll Cake, Pie
                        """)));
                }

                if (allOptions.Length > Choices.Count)
                {
                    return new(new EmbedResult(EmbedFactory.CreateError(
                        $"You can't start a poll with more than {Choices.Count} options 😕")));
                }

                return new(new PageMessageResult(new PageMessage(new(
                    new SinglePageEmbedRenderer(new EmbedBuilder()
                        .WithColor(PollSlashCommand.PollColor)
                        .WithDescription(
                            $"""
                            ## Poll in <#{Context.Channel.Id}>
                            {string.Join('\n', allOptions.Select((option, index) =>
                                $"### {Choices[index]} {option}"))}
                            """)
                        .WithFooter("React to vote!")
                    .Build()),
                    AdditionalReacts: Choices.Take(allOptions.Length).ToList()
                ))));
            },
            Preconditions:
            [
                new InGuildPrecondition(),
                new TaylorBotHasPermissionPrecondition(GuildPermission.AddReactions),
            ]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}

public class SinglePageEmbedRenderer(Embed embed) : IPageMessageRenderer
{
    public bool HasMultiplePages => false;

    public MessageContent RenderNext() => throw new NotImplementedException();

    public MessageContent RenderPrevious() => throw new NotImplementedException();

    public MessageContent Render() => new(embed);
}
