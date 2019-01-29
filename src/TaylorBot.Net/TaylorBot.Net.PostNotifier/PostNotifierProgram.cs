using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TaylorBot.Net.Application;
using TaylorBot.Net.Application.Environment;
using TaylorBot.Net.Application.Events;
using TaylorBot.Net.Application.Extensions;
using TaylorBot.Net.PostNotifier.Events;

namespace TaylorBot.Net.PostNotifier
{
    public class PostNotifierProgram
    {
        public async Task MainAsync()
        {
            using (var services = ConfigureServices())
            {
                await new TaylorBotApplication(services).StartAsync();
            }
        }

        private ServiceProvider ConfigureServices()
        {
            var environment = TaylorBotEnvironment.CreateCurrent();

            var config = new ConfigurationBuilder()
                .AddDefaultTaylorBotConfiguration(environment)
                .Build();

            return new ServiceCollection()
                .AddDefaultTaylorBotServices()
                .AddSingleton<IConfiguration>(config)
                .AddTransient<IReadyHandler, ReadyHandler>()
                .BuildServiceProvider();
        }
    }
}
