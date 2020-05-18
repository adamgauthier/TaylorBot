using System.Reflection;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public static class TaylorBotModuleExtensions
    {
        private static readonly MethodInfo SetContextMethod =
            typeof(TaylorBotModule).GetMethod("Discord.Commands.IModuleBase.SetContext", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void SetContext(this TaylorBotModule module, ITaylorBotCommandContext commandContext)
        {
            SetContextMethod.Invoke(module, new object[] { commandContext });
        }
    }
}
