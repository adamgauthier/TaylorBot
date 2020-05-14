using System;

namespace TaylorBot.Net.Core.Logging
{
    public class LogString
    {
        public static string From(string message)
        {
            return new LogString(message).ToString();
        }

        private readonly string message;

        public LogString(string message)
        {
            this.message = message;
        }

        public override string ToString()
        {
            return $"[{DateTime.Now.ToString("O")}] {message}";
        }
    }
}
