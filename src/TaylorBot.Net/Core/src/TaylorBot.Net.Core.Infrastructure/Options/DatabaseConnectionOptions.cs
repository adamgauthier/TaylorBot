namespace TaylorBot.Net.Core.Infrastructure.Options
{
    public class DatabaseConnectionOptions
    {
        public string Host { get; set; } = null!;
        public uint Port { get; set; }
        public string Database { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ApplicationName { get; set; } = null!;
    }
}
