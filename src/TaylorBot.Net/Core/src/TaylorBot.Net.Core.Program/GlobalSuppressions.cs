// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Alive for the entire lifetime of the application", Scope = "member", Target = "~M:TaylorBot.Net.Core.Program.Extensions.ServiceCollectionExtensions.AddTaylorBotApplicationServices(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,Microsoft.Extensions.Hosting.IHostEnvironment)~Microsoft.Extensions.DependencyInjection.IServiceCollection")]
