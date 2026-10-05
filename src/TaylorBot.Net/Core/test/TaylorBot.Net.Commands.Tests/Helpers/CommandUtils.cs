using FakeItEasy;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Tests.Helpers;

public static class CommandUtils
{
    public static CommandMentioner Mentioner
    {
        get
        {
            var repository = A.Fake<IApplicationCommandsRepository>(o => o.Strict());
            A.CallTo(() => repository.GetCommandIdAsync(A<string>.Ignored, A<bool>.Ignored)).Returns(new ValueTask<SnowflakeId?>((SnowflakeId?)null));
            A.CallTo(() => repository.GetGuildCommandIdAsync(A<SnowflakeId>.Ignored, A<string>.Ignored, A<bool>.Ignored)).Returns(new ValueTask<SnowflakeId?>((SnowflakeId?)null));
            return new(new(repository));
        }
    }
}
