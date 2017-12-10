using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Discord.WebSocket;
using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace FavWordBot
{
    class Program
    {
        //this bot does stuff

        private DiscordSocketClient client;

        private CommandService commands;

        private IServiceProvider service;

        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            DiscordSocketConfig config = new DiscordSocketConfig();
            config.MessageCacheSize = 100;
            client = new DiscordSocketClient();
            commands = new CommandService();

            client.Log += Log;

            /**
             * 
             * 
             * IMPORTANT: CHANGE THE TOKEN for different bot accounts
             * 
             * 
             */
            string token = "";//The bot token. Change to log in as another bot

            service = new ServiceCollection().BuildServiceProvider();

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            //Bot currentBot = new Bot(client);
            //currentBot.Init();

            client.MessageReceived += FuckCommand;

            InstallCommands();

            Console.ReadKey();
        }

        private Task Log(LogMessage m)
        {
            Console.WriteLine(m.ToString());
            return Task.CompletedTask;
        }

        private void InstallCommands()
        {
            client.MessageReceived += HandleCommand;

            commands.AddModuleAsync<FavoriteWordModule>();
        }

        private async Task FuckCommand(SocketMessage message)
        {
            String PREFIX = "Fuck ";
            if (message.Author.Id != client.CurrentUser.Id && message.Content.StartsWith(PREFIX))
            {
                await message.Channel.SendMessageAsync(PREFIX + message.Content.Substring(PREFIX.Length, message.Content.Length - PREFIX.Length) + "!");
            }
        }

        private async Task HandleCommand(SocketMessage param)
        {
            var message = param as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            if (!(message.HasCharPrefix('~', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)) || (message.Author.Username == client.CurrentUser.Username)) return;

            var context = new CommandContext(client, message);

            var result = await commands.ExecuteAsync(context, argPos, service);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);

        }
    }
}
