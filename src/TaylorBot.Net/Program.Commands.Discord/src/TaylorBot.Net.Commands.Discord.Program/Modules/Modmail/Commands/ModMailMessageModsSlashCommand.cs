﻿using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Commands;

public record NoOptions; 

public class ModMailMessageModsSlashCommand(
    ILogger<ModMailMessageModsSlashCommand> logger,
    IOptionsMonitor<ModMailOptions> options,
    IModMailBlockedUsersRepository modMailBlockedUsersRepository,
    ModMailChannelLogger modMailChannelLogger) : ISlashCommand<NoOptions>
{
    public static string CommandName => "modmail message-mods";

    public ISlashCommandInfo Info => new ModalCommandInfo(CommandName);

    private static readonly Color EmbedColor = new(255, 255, 240);
    private readonly IOptionsMonitor<ModMailOptions> _options = options;

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                // Creates a modal form with subject and message fields for users to compose their message
                return new(new CreateModalResult(
                    Id: "modmail-message-mods",
                    Title: "Send Message to Moderators",
                    TextInputs: [
                        new TextInput(Id: "subject", Style: TextInputStyle.Short, Label: "Subject", Required: false, MaxLength: 50),
                        new TextInput(Id: "messagecontent", Style: TextInputStyle.Paragraph, Label: "Message to moderators", Required: true)
                    ],
                    SubmitAction: SubmitAsync,
                    IsPrivateResponse: true
                ));

                // Processes the submitted message and shows a confirmation prompt
                ValueTask<MessageResult> SubmitAsync(ModalSubmit submit)
                {
                    var subject = submit.TextInputs.Single(t => t.CustomId == "subject").Value;
                    if (string.IsNullOrWhiteSpace(subject))
                    {
                        subject = "No Subject";
                    }

                    var messageContent = submit.TextInputs.Single(t => t.CustomId == "messagecontent").Value;

                    // Creates an embed containing the user's message and metadata
                    var embed = new EmbedBuilder()
                        .WithColor(EmbedColor)
                        .WithTitle(subject)
                        .WithDescription(messageContent)
                        .AddField("From", context.User.FormatTagAndMention(), inline: true)
                        .WithFooter("Mod mail received", iconUrl: _options.CurrentValue.ReceivedLogEmbedFooterIconUrl)
                        .WithCurrentTimestamp()
                        .Build();

                    return new(MessageResult.CreatePrompt(
                        new(new[] { embed, EmbedFactory.CreateWarning($"Are you sure you want to send the above message to the moderation team of '{guild.Name}'?") }),
                        confirm: async () => new(await SendAsync(embed))
                    ));
                }

                // Handles message delivery to the moderation team's channel
                async ValueTask<Embed> SendAsync(Embed embed)
                {
                    var isBlocked = await modMailBlockedUsersRepository.IsBlockedAsync(guild, context.User);
                    if (isBlocked)
                    {
                        return EmbedFactory.CreateError("Sorry, the moderation team has blocked you from sending mod mail. 😕");
                    }

                    var channel = await modMailChannelLogger.GetModMailLogAsync(guild);

                    if (channel != null)
                    {
                        try
                        {
                            await channel.SendMessageAsync(embed: embed);
                            return EmbedFactory.CreateSuccess(
                                $"""
                                Message sent to the moderation team of '{guild.Name}'. ✉
                                If you're expecting a response, **make sure you are able to send and receive DMs from TaylorBot**.
                                """);
                        }
                        catch (Exception e)
                        {
                            logger.LogWarning(e, "Error occurred when sending mod mail in {Guild}:", guild.FormatLog());
                        }
                    }

                    return EmbedFactory.CreateError(
                        $"""
                        I was not able to send the message to the moderation team. 😕
                        Make sure they have a moderation log set up with {context.MentionCommand("mod log set")} and TaylorBot has access to it.
                        """);
                }
            },
            Preconditions: [
                new InGuildPrecondition(botMustBeInGuild: true)
            ]
        ));
    }
}
