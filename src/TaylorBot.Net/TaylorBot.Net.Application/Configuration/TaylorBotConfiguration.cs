using Microsoft.Extensions.Configuration;
using TaylorBot.Net.Core.Configuration;

namespace TaylorBot.Net.Application.Configuration
{
    public class TaylorBotConfiguration : IShardCountProvider, ITokenProvider
    {
        private readonly IConfiguration configuration;

        public TaylorBotConfiguration(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GetDiscordToken()
        {
            return configuration["discord:token"];
        }

        public int GetShardCount()
        {
            return int.Parse(configuration["discord:shardCount"]);
        }
    }
}
