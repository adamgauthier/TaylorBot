﻿using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Program;

namespace TaylorBot.Net.Commands
{
    public class TaylorBotCommandHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TaylorBotHostedService _taylorBotHostedService;

        public TaylorBotCommandHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _taylorBotHostedService = new TaylorBotHostedService(serviceProvider);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var commandService = _serviceProvider.GetRequiredService<CommandService>();
            var commandExecutedHandler = _serviceProvider.GetRequiredService<CommandExecutedHandler>();

            commandService.CommandExecuted += commandExecutedHandler.OnCommandExecutedAsync;

            commandService.AddTypeReader<IUser>(new CustomUserTypeReader<IUser>(), replaceDefault: true);

            await commandService.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _serviceProvider
            );

            await _taylorBotHostedService.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _taylorBotHostedService.StopAsync(cancellationToken);
        }
    }
}