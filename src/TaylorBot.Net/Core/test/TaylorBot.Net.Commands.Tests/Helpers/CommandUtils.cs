using FakeItEasy;

namespace TaylorBot.Net.Commands.Tests.Helpers;

public class CommandUtils
{
    public static CommandMentioner Mentioner
    {
        get
        {
            var repository = A.Fake<IApplicationCommandsRepository>(o => o.Strict());
            A.CallTo(() => repository.GetCommandId(A<string>.Ignored)).Returns(null);
            return new(repository);
        }
    }
}
