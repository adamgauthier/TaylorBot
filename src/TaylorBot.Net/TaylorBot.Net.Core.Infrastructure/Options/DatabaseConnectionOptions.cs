namespace TaylorBot.Net.Core.Infrastructure.Options
{
    public class DatabaseConnectionOptions
    {
        public string Host { get; set; }
        public uint Port { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
