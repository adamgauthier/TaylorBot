using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.EntityTracker.Domain.User;

namespace TaylorBot.Net.Commands.Preconditions
{
    public class GetUserIgnoreUntilResult : IUserAddedResult
    {
        public DateTimeOffset IgnoreUntil { get; }

        public bool WasAdded { get; }

        public bool WasUsernameChanged { get; }

        public string? PreviousUsername { get; }

        public GetUserIgnoreUntilResult(DateTimeOffset ignoreUntil, bool wasAdded, bool wasUsernameChanged, string? previousUsername)
        {
            IgnoreUntil = ignoreUntil;
            WasAdded = wasAdded;
            WasUsernameChanged = wasUsernameChanged;
            PreviousUsername = previousUsername;
        }
    }

    public interface IIgnoredUserRepository
    {
        ValueTask<GetUserIgnoreUntilResult> InsertOrGetUserIgnoreUntilAsync(IUser user);
        ValueTask IgnoreUntilAsync(IUser user, DateTimeOffset until);
    }

    public class RequireUserNotIgnoredAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var ignoredUserRepository = services.GetRequiredService<IIgnoredUserRepository>();
            var usernameTrackerDomainService = services.GetRequiredService<UsernameTrackerDomainService>();

            var getUserIgnoreUntilResult = await ignoredUserRepository.InsertOrGetUserIgnoreUntilAsync(context.User);

            await usernameTrackerDomainService.AddUsernameAfterUserAddedAsync(context.User, getUserIgnoreUntilResult);

            return DateTimeOffset.Now < getUserIgnoreUntilResult.IgnoreUntil ?
                TaylorBotPreconditionResult.FromPrivateError($"user is ignored until {getUserIgnoreUntilResult.IgnoreUntil:o}") :
                PreconditionResult.FromSuccess();
        }
    }
}
