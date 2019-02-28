using System;

namespace TaylorBot.Net.Core.Program.Environment
{
    public class TaylorBotEnvironment
    {
        private const string ENVIRONMENT_VARIABLE = "taylorbot:Environment";

        public static TaylorBotEnvironment CreateCurrent()
        {
            var environment = System.Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE) ?? throw new InvalidOperationException($"Could not find {ENVIRONMENT_VARIABLE} environment variable.");

            return new TaylorBotEnvironment(environment);
        }

        private enum TaylorBotEnvironmentValue
        {
            Development,
            Production
        }

        private readonly TaylorBotEnvironmentValue value;

        private TaylorBotEnvironment(string environment)
        {
            if (!Enum.TryParse(environment, ignoreCase: true, out TaylorBotEnvironmentValue value))
                throw new ArgumentOutOfRangeException(nameof(environment));
            this.value = value;
        }

        public override string ToString() => value.ToString();
    }
}
