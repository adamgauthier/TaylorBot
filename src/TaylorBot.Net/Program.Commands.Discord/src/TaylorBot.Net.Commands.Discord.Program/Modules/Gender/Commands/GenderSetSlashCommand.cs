﻿using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;

public class GenderSetSlashCommand(IGenderRepository genderRepository) : ISlashCommand<GenderSetSlashCommand.Options>
{
    public static string CommandName => "gender set";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedString gender);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await genderRepository.SetGenderAsync(context.User, options.gender.Value);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Your gender has been set to {options.gender.Value}. ✅
                    You are now included in </server population:1137547317549998130> stats for servers you're in. 🧮
                    People can now use {context.MentionSlashCommand("gender show")} to see your gender. 👁️
                    """));
            }
        ));
    }
}
