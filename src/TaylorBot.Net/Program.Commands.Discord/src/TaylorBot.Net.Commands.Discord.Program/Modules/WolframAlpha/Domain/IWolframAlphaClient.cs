using static TaylorBot.Net.Commands.Discord.Program.Modules.WolframAlpha.Domain.WolframAlphaResult;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.WolframAlpha.Domain;

public interface IWolframAlphaResult { }
public record WolframAlphaResult(Pod InputPod, Pod OutputPod) : IWolframAlphaResult
{
    public record Pod(string PlainText, string ImageUrl);
};
public record GenericWolframAlphaError() : IWolframAlphaResult;
public record WolframAlphaFailed() : IWolframAlphaResult;

public interface IWolframAlphaClient
{
    ValueTask<IWolframAlphaResult> QueryAsync(string query);
}
