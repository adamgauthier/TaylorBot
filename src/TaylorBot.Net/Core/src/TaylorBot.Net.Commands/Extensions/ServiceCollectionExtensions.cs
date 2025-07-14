using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Events;
using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.Parsers.Roles;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.StringMappers;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Program.Extensions;
using TaylorBot.Net.Core.Tasks;

namespace TaylorBot.Net.Commands.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommandApplication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        services.AddHttpClient<IInteractionResponseClient, InteractionResponseClient>((provider, client) =>
        {
            client.BaseAddress = new Uri("https://discord.com/api/v10/");
        });

        return services
            .AddTaylorBotApplicationServices(configuration)
            .AddSingleton(services)
            .AddTransient<CommandActivityFactory>()
            .AddSingleton(provider => new CommandService(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
            }))
            .AddTransient<CommandPrefixDomainService>()
            .AddTransient<DisabledGuildCommandDomainService>()
            .AddTransient<SingletonTaskRunner>()
            .AddTransient<IShardReadyHandler, ShardReadyHandler>()
            .AddTransient<IUserMessageReceivedHandler, CommandHandler>()
            .AddTransient<PermissionStringMapper>()
            .AddTransient<InteractionMapper>()
            .AddTransient<RunContextFactory>()
            .AddTransient<IApplicationCommandsRepository, ApplicationCommandsRepository>()
            .AddTransient<CommandMentioner>()
            .AddSingleton<ApplicationCommandsRepository>()
            .AddTransient<SlashCommandHandler>()
            .AddTransient<MessageComponentHandler>()
            .AddTransient<ModalInteractionHandler>()
            .AddButtonHandler<GenericPromptCancelButtonHandler>()
            .AddButtonHandler<GenericMessageDeleteButtonHandler>()
            .AddTransient<IInteractionCreatedHandler, InteractionCreatedHandler>()
            .AddSingleton<PageMessageReactionsHandler>()
            .AddTransient<IReactionAddedHandler>(c => c.GetRequiredService<PageMessageReactionsHandler>())
            .AddTransient<IReactionRemovedHandler>(c => c.GetRequiredService<PageMessageReactionsHandler>())
            .AddTransient<CommandExecutedHandler>()
            .AddTransient<PrefixedCommandRunner>()
            .AddTransient<CommandServiceLogger>()
            .AddTransient<IRateLimiter, CommandRateLimiter>()
            .AddTransient<IUserTracker, UserTracker>()
            .AddTransient<MentionedUserTypeReader<IUser>>()
            .AddTransient<CustomUserTypeReader<IUser>>()
            .AddTransient<MentionedUserTypeReader<IGuildUser>>()
            .AddTransient<CustomUserTypeReader<IGuildUser>>()
            .AddTransient<MentionedUserNotAuthorTypeReader<IGuildUser>>()
            .AddTransient<MentionedUserNotAuthorTypeReader<IUser>>()
            .AddTransient<MentionedUsersNotAuthorTypeReader<IUser>>()
            .AddTransient<MentionedUserNotAuthorOrClientTypeReader<IGuildUser>>()
            .AddTransient<CustomRoleTypeReader<IRole>>()
            .AddTransient<RoleNotEveryoneTypeReader<IRole>>()
            .AddTransient<CustomChannelTypeReader<IChannel>>()
            .AddTransient<CustomChannelTypeReader<ITextChannel>>()
            .AddTransient<PositiveInt32.Factory>()
            .AddTransient<WordTypeReader>()
            .AddTransient<CommandTypeReader>()
            .AddTransient<NotDisabledPrecondition>()
            .AddTransient<NotGuildDisabledPrecondition>()
            .AddTransient<NotGuildChannelDisabledPrecondition>()
            .AddTransient<UserNotIgnoredPrecondition>()
            .AddTransient<MemberTrackedPrecondition>()
            .AddTransient<TextChannelTrackedPrecondition>()
            .AddTransient<UserNoOngoingCommandPrecondition>()
            .AddTransient<NotRemovedPrecondition>()
            .AddTransient<UserHasPermissionOrOwnerPrecondition.Factory>()
            .AddTransient<TaylorBotHasPermissionPrecondition.Factory>()
            .AddTransient<TaylorBotOwnerPrecondition>()
            .AddTransient<InGuildPrecondition.Factory>()
            .AddTransient<PlusPrecondition.Factory>()
            .AddTransient<ICommandRunner, CommandRunner>()
            .AddOptionParser<StringParser, ParsedString>()
            .AddOptionParser<OptionalStringParser, ParsedOptionalString>()
            .AddOptionParser<OptionalBooleanParser, ParsedOptionalBoolean>()
            .AddOptionParser<UserParser, ParsedUser>()
            .AddOptionParser<UserNotAuthorParser, ParsedUserNotAuthor>()
            .AddOptionParser<UserNotAuthorAndBotParser, ParsedUserNotAuthorAndBot>()
            .AddOptionParser<UserNotAuthorAndTaylorBotParser, ParsedUserNotAuthorAndTaylorBot>()
            .AddOptionParser<UserOptionalParser, ParsedUserOptional>()
            .AddOptionParser<UserOrAuthorParser, ParsedUserOrAuthor>()
            .AddOptionParser<MemberParser, ParsedMember>()
            .AddOptionParser<MemberNotAuthorParser, ParsedMemberNotAuthor>()
            .AddOptionParser<MemberNotAuthorAndTaylorBotParser, ParsedMemberNotAuthorAndTaylorBot>()
            .AddOptionParser<MemberNotAuthorAndBotParser, ParsedMemberNotAuthorAndBot>()
            .AddOptionParser<MemberOrAuthorParser, ParsedMemberOrAuthor>()
            .AddOptionParser<ChannelOrCurrentParser, ParsedChannelOrCurrent>()
            .AddOptionParser<TextChannelOrCurrentParser, ParsedTextChannelOrCurrent>()
            .AddOptionParser<NonThreadTextChannellOrCurrentParser, ParsedNonThreadTextChannelOrCurrent>()
            .AddOptionParser<RoleParser, ParsedRole>()
            .AddOptionParser<TimeSpanParser, ParsedTimeSpan>()
            .AddOptionParser<IntegerParser, ParsedInteger>()
            .AddOptionParser<OptionalIntegerParser, ParsedOptionalInteger>()
            .AddOptionParser<PositiveIntegerParser, ParsedPositiveInteger>()
            .AddOptionParser<AttachmentParser, ParsedAttachment>()
            .AddOptionParser<OptionalAttachmentParser, ParsedOptionalAttachment>()
            .AddMemoryCache()
            .AddTransient<PageOptionsInMemoryRepository>()
            .AddTransient<PageMessageFactory>()
            .AddButtonHandler<PagePreviousButtonHandler>()
            .AddButtonHandler<PageNextButtonHandler>()
            .AddButtonHandler<PageCancelButtonHandler>()
            ;
    }

    public static IServiceCollection AddOptionParser<TParser, TOption>(this IServiceCollection services)
        where TParser : class, IOptionParser<TOption>
    {
        return services
            .AddTransient<TParser>()
            .AddTransient<IOptionParser<TOption>, TParser>();
    }

    public static IServiceCollection AddSlashCommand<T>(this IServiceCollection services)
        where T : class, ISlashCommand, IKeyedSlashCommand
    {
        return services
            .AddTransient<T>()
            .AddKeyedTransient<ISlashCommand, T>(T.CommandName);
    }

    public static IServiceCollection AddButtonHandler<T>(this IServiceCollection services)
        where T : class, IButtonHandler
    {
        return services
            .AddTransient<T>()
            .AddKeyedTransient<IButtonComponentHandler, T>(T.CustomIdName.ToText());
    }

    public static IServiceCollection AddStringSelectHandler<T>(this IServiceCollection services)
        where T : class, IStringSelectHandler
    {
        return services
            .AddTransient<T>()
            .AddKeyedTransient<IStringSelectComponentHandler, T>(T.CustomIdName.ToText());
    }

    public static IServiceCollection AddUserSelectHandler<T>(this IServiceCollection services)
        where T : class, IUserSelectHandler
    {
        return services
            .AddTransient<T>()
            .AddKeyedTransient<IUserSelectComponentHandler, T>(T.CustomIdName.ToText());
    }

    public static IServiceCollection AddChannelSelectHandler<T>(this IServiceCollection services)
        where T : class, IChannelSelectHandler
    {
        return services
            .AddTransient<T>()
            .AddKeyedTransient<IChannelSelectComponentHandler, T>(T.CustomIdName.ToText());
    }

    public static IServiceCollection AddModalHandler<T>(this IServiceCollection services)
        where T : class, IModalHandler
    {
        return services
            .AddTransient<T>()
            .AddKeyedTransient<IModalComponentHandler, T>(T.CustomIdName.ToText());
    }
}
