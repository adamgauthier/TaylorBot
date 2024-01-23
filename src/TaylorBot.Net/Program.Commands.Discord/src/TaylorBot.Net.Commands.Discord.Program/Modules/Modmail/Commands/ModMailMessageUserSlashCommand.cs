using Discord;
using Discord.Net;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Commands;

public class ModMailMessageUserSlashCommand(ModMailChannelLogger modMailChannelLogger, IOptionsMonitor<ModMailOptions> modMailOptions) : ISlashCommand<ModMailMessageUserSlashCommand.Options>
{
    public ISlashCommandInfo Info => new ModalCommandInfo("modmail message-user");

    public record Options(ParsedMemberNotAuthorAndBot user);

    private static readonly Color EmbedColor = new(255, 255, 240);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            () =>
            {
                return new(new CreateModalResult(
                    Id: "modmail-message-user",
                    Title: "Send Mod Mail to User",
                    TextInputs: [new TextInput(Id: "messagecontent", TextInputStyle.Paragraph, Label: "Message to user")],
                    SubmitAction: SubmitAsync,
                    IsPrivateResponse: true
                ));

                ValueTask<MessageResult> SubmitAsync(ModalSubmit submit)
                {
                    var messageContent = submit.TextInputs.Single(t => t.CustomId == "messagecontent").Value;
                    var user = options.user.Member;

                    var embed = new EmbedBuilder()
                        .WithGuildAsAuthor(user.Guild)
                        .WithColor(EmbedColor)
                        .WithTitle("Message from the moderation team")
                        .WithDescription(messageContent)
                        .WithFooter("Reply with /mod mail message-mods")
                    .Build();

                    return new(MessageResult.CreatePrompt(
                        new([embed, EmbedFactory.CreateWarning($"Are you sure you want to send the above message to {user.FormatTagAndMention()}?")]),
                        confirm: async () => new(await SendAsync(user, embed))
                    ));
                }

                async ValueTask<Embed> SendAsync(IGuildUser user, Embed embed)
                {
                    try
                    {
                        await user.SendMessageAsync(embed: embed);
                    }
                    catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                    {
                        return EmbedFactory.CreateError(
                            $"""
                            I couldn't send this message to {user.FormatTagAndMention()} because their DM settings won't allow it. ❌
                            The user must have 'Allow direct messages from server members' enabled and must not have TaylorBot blocked.
                            """);
                    }

                    var wasLogged = await modMailChannelLogger.TrySendModMailLogAsync(user.Guild, context.User, user, logEmbed =>
                        logEmbed
                            .WithColor(EmbedColor)
                            .WithTitle("Message")
                            .WithDescription(embed.Description)
                            .WithFooter("Mod mail sent", iconUrl: modMailOptions.CurrentValue.SentLogEmbedFooterIconUrl)
                    );

                    return modMailChannelLogger.CreateResultEmbed(context, wasLogged, $"Message sent to {user.FormatTagAndMention()} ✉️");
                }
            },
            Preconditions: [
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.BanMembers),
            ]
        ));
    }
}
