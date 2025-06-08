using Discord;
using System.Text.RegularExpressions;
using TaylorBot.Net.Commands.Discord.Program.Modules.Help.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Strings;
using SelectMenuOption = TaylorBot.Net.Commands.PostExecution.InteractionResponse.SelectMenuOption;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Help.Commands;

public class HelpSlashCommand(IBotInfoRepository botInfoRepository, CommandCategoryService categoryService, CommandMentioner mention) : ISlashCommand<NoOptions>
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
                return new MessageResult(await GetHelpResponseAsync(context));
            },
            Preconditions: BuildPreconditions()
        ));
    }

    public async Task<MessageResponse> GetHelpResponseAsync(RunContext context, bool isSlashCommand = true)
    {
        var productVersion = await botInfoRepository.GetProductVersionAsync();
        var applicationInfo = await context.Client.GetApplicationInfoAsync();

        var embed = new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithThumbnailUrl(applicationInfo.IconUrl)
            .WithDescription(
                $"""
                # TaylorBot {productVersion} ⭐
                {"TaylorBot".DiscordMdLink("https://taylorbot.app/")} is a multi-purpose Discord bot created with love in **November 2015** by {applicationInfo.Owner.Mention} 💖
                {applicationInfo.Description}
                {(isSlashCommand
                    ? "### Pick a command category below to learn more 👇"
                    : $"### Use {mention.SlashCommand("help")} to learn more about commands! ✨")}                
                """)
            .Build();

        if (!isSlashCommand)
        {
            return new(embed);
        }

        return new(new([embed]), [await CreateCategorySelectAsync("home")]);
    }

    public async Task<InteractionComponent> CreateCategorySelectAsync(string? selectedCategoryId = null)
    {
        var categories = (await categoryService.GetAllCategoriesAsync()).ToList();
        var selectOptions = categories.Select(category => new SelectMenuOption(
            label: category.Name,
            value: category.Id,
            emoji: new(category.Emoji),
            default_value: category.Id == selectedCategoryId
        )).ToList();

        SelectMenuOption home = new(
            label: "Home",
            value: "home",
            emoji: new("🏠"),
            default_value: selectedCategoryId == "home");

        return InteractionComponent.CreateStringSelect(
            custom_id: InteractionCustomId.Create(HelpCategoryHandler.CustomIdName).RawId,
            placeholder: "Select a category",
            options: [home, .. selectOptions]);
    }
}

public partial class HelpCategoryHandler(IInteractionResponseClient interactionResponseClient, CommandCategoryService categoryService, HelpSlashCommand helpCommand, CommandMentioner mention) : IStringSelectHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.HelpCategory;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        RequireOriginalUser: true);

    [GeneratedRegex(@"`/([^`]+)`")]
    private static partial Regex SlashCommandRegex();

    private string ReplaceSlashCommandMentions(string markdown)
    {
        return SlashCommandRegex().Replace(markdown, match => mention.SlashCommand(match.Groups[1].Value));
    }

    public async Task HandleAsync(DiscordStringSelectComponent select, RunContext context)
    {
        var categoryId = select.Values[0];
        if (categoryId == "home")
        {
            var response = await helpCommand.GetHelpResponseAsync(context);
            await interactionResponseClient.EditOriginalResponseAsync(select.Interaction, response);
            return;
        }

        var categoryInfo = await categoryService.GetCategoryAsync(categoryId);

        var embed = EmbedFactory.CreateSuccess(
            $"""
            # {categoryInfo.Name} {categoryInfo.Emoji}
            {ReplaceSlashCommandMentions(categoryInfo.Description)}
            """);
        var selectMenu = await helpCommand.CreateCategorySelectAsync(categoryId);

        await interactionResponseClient.EditOriginalResponseAsync(select.Interaction, new MessageResponse(new([embed]), [selectMenu]));
    }
}
