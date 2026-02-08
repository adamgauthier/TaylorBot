using FakeItEasy;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Tests.Helpers;

public static class CommandUtils
{
    public static CommandMentioner Mentioner
    {
        get
        {
            var repository = A.Fake<IApplicationCommandsRepository>(o => o.Strict());
            A.CallTo(() => repository.GetCommandId(A<string>.Ignored)).Returns(null);
            A.CallTo(() => repository.GetGuildCommandId(A<SnowflakeId>.Ignored, A<string>.Ignored)).Returns(null);
            return new(repository);
        }
    }
}
