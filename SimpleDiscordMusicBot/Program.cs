using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Discord.WebSocket;
using System.IO;
using System;
using System.Threading.Tasks;
using Discord.Addons.Hosting;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MusicBot.Services;

namespace MusicBot
{
    class Program
    {
        static async Task Main()
        {
            var builder = new HostBuilder() //Just Host builder
                .ConfigureAppConfiguration(x =>
                {
                    var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true) //path to config file
                    .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureDiscordHost((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Debug,
                        AlwaysDownloadUsers = false,
                        MessageCacheSize = 200,
                    };
                    config.Token = context.Configuration["Token"];
                })
                .UseCommandService((context, config) =>
                {
                    config.CaseSensitiveCommands = false;
                    config.LogLevel = LogSeverity.Debug;
                    config.DefaultRunMode = RunMode.Sync; 
                })
                .ConfigureServices((context, services) => //Add services here
                {
                    services
                     .AddHostedService<CommandHandler>()
                     .AddSingleton<AudioService>();
                     
                    
                })
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}
