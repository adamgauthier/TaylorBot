﻿using Discord;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;

public class GenderShowSlashCommand(IGenderRepository genderRepository) : ISlashCommand<GenderShowSlashCommand.Options>
{
    public const string PrefixCommandName = "gender";

    public static string CommandName => "gender show";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserOrAuthor user);

    public Command Show(DiscordUser user, RunContext? context = null) => new(
        new(Info.Name, Aliases: [PrefixCommandName]),
        async () =>
        {
            var gender = await genderRepository.GetGenderAsync(user);

            if (gender != null)
            {
                var embed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithUserAsAuthor(user)
                    .WithDescription(
                        $"""
                        {user.Mention}'s gender is **{gender}**. 🆔
                        """);

                return new EmbedResult(embed.Build());
            }
            else
            {
                return new EmbedResult(EmbedFactory.CreateError(
                    $"""
                    {user.Mention}'s gender is not set. 🚫
                    They need to use {context?.MentionSlashCommand("gender set") ?? "</gender set:1150180971224764510>"} to set it first.
                    """));
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Show(options.user.User, context));
    }
}
