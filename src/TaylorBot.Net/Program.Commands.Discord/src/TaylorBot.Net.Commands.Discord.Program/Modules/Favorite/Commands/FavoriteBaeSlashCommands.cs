﻿using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;

public interface IBaeRepository
{
    ValueTask<string?> GetBaeAsync(IUser user);
    ValueTask SetBaeAsync(IUser user, string bae);
    ValueTask ClearBaeAsync(IUser user);
}

public class FavoriteBaeShowSlashCommand : ISlashCommand<FavoriteBaeShowSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("favorite bae show");

    public record Options(ParsedUserOrAuthor user);

    private readonly IBaeRepository _baeRepository;

    public FavoriteBaeShowSlashCommand(IBaeRepository favoriteBaeRepository)
    {
        _baeRepository = favoriteBaeRepository;
    }

    public static Embed BuildDisplayEmbed(IUser user, string favoriteBae)
    {
        return new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithUserAsAuthor(user)
            .WithTitle("Bae ❤️")
            .WithDescription(favoriteBae)
            .Build();
    }

    public Command Show(IUser user, RunContext? context = null) => new(
        new(Info.Name),
        async () =>
        {
            var favoriteBae = await _baeRepository.GetBaeAsync(user);

            if (favoriteBae != null)
            {
                Embed embed = BuildDisplayEmbed(user, favoriteBae);
                return new EmbedResult(embed);
            }
            else
            {
                return new EmbedResult(EmbedFactory.CreateError(
                    $"""
                    {user.Mention}'s bae is not set. 🚫
                    They need to use {context?.MentionCommand("favorite bae set") ?? "</favorite bae set:1169468169140838502>"} to set it first.
                    """));
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Show(options.user.User, context));
    }
}

public class FavoriteBaeSetSlashCommand : ISlashCommand<FavoriteBaeSetSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("favorite bae set");

    public record Options(ParsedString bae);

    private readonly IBaeRepository _baeRepository;

    public FavoriteBaeSetSlashCommand(IBaeRepository favoriteBaeRepository)
    {
        _baeRepository = favoriteBaeRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        Command command = new(
            new(Info.Name),
            () =>
            {
                var embed = FavoriteBaeShowSlashCommand.BuildDisplayEmbed(context.User, options.bae.Value);

                return new(MessageResult.CreatePrompt(
                    new(new[] { embed, EmbedFactory.CreateWarning("Are you sure you want to set your bae to the above?") }),
                    confirm: async () => new(await SetAsync(options.bae.Value))
                ));

                async ValueTask<Embed> SetAsync(string favoriteBae)
                {
                    await _baeRepository.SetBaeAsync(context.User, options.bae.Value);

                    return EmbedFactory.CreateSuccess(
                        $"""
                        Your bae has been set successfully. ✅
                        Others can now use {context?.MentionCommand("favorite bae show") ?? "</favorite bae show:1169468169140838502>"} to see your bae. ❤️
                        """);
                }
            }
        );
        return new(command);
    }
}

public class FavoriteBaeClearSlashCommand : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("favorite bae clear");

    private readonly IBaeRepository _baeRepository;

    public FavoriteBaeClearSlashCommand(IBaeRepository favoriteBaeRepository)
    {
        _baeRepository = favoriteBaeRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await _baeRepository.ClearBaeAsync(context.User);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Your bae has been cleared and is no longer visible. ✅
                    You can set it again with {context.MentionCommand("favorite bae set")}.
                    """));
            }
        ));
    }
}
