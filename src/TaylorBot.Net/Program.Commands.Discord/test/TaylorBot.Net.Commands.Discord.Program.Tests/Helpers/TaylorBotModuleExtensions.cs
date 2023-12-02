using Discord.Commands;
using System.Reflection;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;

public static class TaylorBotModuleExtensions
{
    private static readonly MethodInfo SetContextMethod =
        typeof(TaylorBotModule).GetMethod("Discord.Commands.IModuleBase.SetContext", BindingFlags.Instance | BindingFlags.NonPublic)!;

    public static void SetContext(this TaylorBotModule module, ITaylorBotCommandContext commandContext)
    {
        SetContextMethod.Invoke(module, new object[] { commandContext });
    }

    public static T GetResult<T>(this RuntimeResult result)
    {
        return (T)((TaylorBotResult)result).Result;
    }
}
