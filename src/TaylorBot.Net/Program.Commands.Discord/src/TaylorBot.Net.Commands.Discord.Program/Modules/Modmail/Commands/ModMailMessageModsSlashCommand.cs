﻿using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Commands;

public class ModMailMessageModsSlashCommand : ISlashCommand<ModMailMessageModsSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("modmail message-mods", IsPrivateResponse: true);

    public record Options(ParsedString message);

    private static readonly Color EmbedColor = new(255, 255, 240);

    private readonly ILogger<ModMailMessageModsSlashCommand> _logger;
    private readonly IOptionsMonitor<ModMailOptions> _options;
    private readonly IModMailBlockedUsersRepository _modMailBlockedUsersRepository;
    private readonly ModMailChannelLogger _modMailChannelLogger;

    public ModMailMessageModsSlashCommand(ILogger<ModMailMessageModsSlashCommand> logger, IOptionsMonitor<ModMailOptions> options, IModMailBlockedUsersRepository modMailBlockedUsersRepository, ModMailChannelLogger modMailChannelLogger)
    {
        _logger = logger;
        _options = options;
        _modMailBlockedUsersRepository = modMailBlockedUsersRepository;
        _modMailChannelLogger = modMailChannelLogger;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            () =>
            {
                var guild = context.Guild!;

                var embed = new EmbedBuilder()
                    .WithColor(EmbedColor)
                    .WithTitle("Message")
                    .WithDescription(options.message.Value)
                    .AddField("From", context.User.FormatTagAndMention(), inline: true)
                    .WithFooter("Mod mail received", iconUrl: _options.CurrentValue.ReceivedLogEmbedFooterIconUrl)
                    .WithCurrentTimestamp()
                .Build();

                return new(MessageResult.CreatePrompt(
                    new(new[] { embed, EmbedFactory.CreateWarning($"Are you sure you want to send the above message to the moderation team of '{guild.Name}'?") }),
                    confirm: async () => new(await SendAsync())
                ));

                async ValueTask<Embed> SendAsync()
                {
                    var isBlocked = await _modMailBlockedUsersRepository.IsBlockedAsync(guild, context.User);
                    if (isBlocked)
                    {
                        return EmbedFactory.CreateError("Sorry, the moderation team has blocked you from sending mod mail. 😕");
                    }

                    var channel = await _modMailChannelLogger.GetModMailLogAsync(guild);

                    if (channel != null)
                    {
                        try
                        {
                            await channel.SendMessageAsync(embed: embed);
                            return EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                                $"Message sent to the moderation team of '{guild.Name}'. ✉",
                                "If you're expecting a response, **make sure you are able to send and receive DMs from TaylorBot**."
                            }));
                        }
                        catch (Exception e)
                        {
                            _logger.LogWarning(e, "Error occurred when sending mod mail in {Guild}:", guild.FormatLog());
                        }
                    }

                    return EmbedFactory.CreateError(string.Join('\n', new[] {
                        "I was not able to send the message to the moderation team. 😕",
                        $"Make sure they have a moderation log set up with {context.MentionCommand("mod log set")} and TaylorBot has access to it.",
                    }));
                }
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition()
            }
        ));
    }
}