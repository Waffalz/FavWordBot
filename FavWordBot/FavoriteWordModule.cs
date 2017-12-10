using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace FavWordBot
{
    public class FavoriteWordModule : ModuleBase
    {

        int MAX_DEPTH = 50; //the number of previous messages the bot will sift through
        string[] WORD_BLACKLIST = { "the", "in", "it", "to", "he", "she", "his", "her", "and", "but", "are", "when", "i", "a", "this", "of", "be", "if", "do", "that", "is", "or", "for", "i\'m" }; //words to blacklist from the word leaderboards
        char[] TO_TRIM = { ' ', '\t', '.', ',', ':', '/', '\\', '\'', '\"', '\n', '(', ')', '[', ']', '{', '}', '<', '>', '!', '?', ';' }; //array of characters to trim off the edges of each word
        char[] TO_SPLIT = { ' ', '\t', '.', ',', ':', '/', '\\', '\"', '\n' }; //array of characters to take out
        int LEADERBOARD_COUNT = 6; //number of words to display per user

        public FavoriteWordModule()
        {

        }

        [Command("FWord"), Summary("Displays frequency of words")]
        public async Task FavoriteWord([Summary("The channel get data from")] IMessageChannel chan, [Summary("Number of previous messages to get data from")] int depth, [Summary("Number of most frequent words to display")] int leadCount)
        {
            Console.WriteLine("starting Favorite Word");

            if (depth <= 0) return;

            Dictionary<IUser, Dictionary<string, int>> bigList = new Dictionary<IUser, Dictionary<string, int>>();

            var messageCollection = await chan.GetMessagesAsync(depth, Discord.CacheMode.AllowDownload, Discord.RequestOptions.Default).Flatten();

            //Back when I thought there was a max number of messages getMessagesAsync could get (100). I guess this bit of code isn't necessary 
            /*
            int linesLeft = depth;
            List<IMessage> messageList = new List<IMessage>();
            IMessage lastMessage;

            //getting the latest message
            var mesSingle = await chan.GetMessagesAsync(1, Discord.CacheMode.AllowDownload, Discord.RequestOptions.Default).Flatten();
            IEnumerator<IMessage> iterator = mesSingle.GetEnumerator();
            iterator.MoveNext();

            IMessage mes = iterator.Current;
            messageList.Add(mes);
            lastMessage = mes;

            linesLeft--;

            //Because the limit of 
            while (linesLeft > 0)
            {
                int limit = Math.Min(linesLeft, 100);
                var messageCollection = await chan.GetMessagesAsync(lastMessage, Direction.Before, limit, Discord.CacheMode.AllowDownload, Discord.RequestOptions.Default).Flatten();

                int count = 0;
                foreach (IMessage messa in messageCollection)
                {
                    count++;
                    messageList.Add(messa);
                    if (count >= 100) lastMessage = messa;
                    Console.WriteLine(count + ". " + messa.Content + " || " + lastMessage.Content);
                }
                linesLeft -= count;
            }
            */

            foreach (IMessage currentMessage in messageCollection)
            {
                if (currentMessage.Content.StartsWith("~")) continue;
                Dictionary<string, int> userWordList;
                IUser author = currentMessage.Author;
                if (!bigList.TryGetValue(author, out userWordList))
                {
                    userWordList = new Dictionary<string, int>();
                    bigList[author] = userWordList;
                }

                string[] words = currentMessage.Content.ToLower().Split(TO_SPLIT);

                foreach (string word in words)
                {
                    word.Trim(TO_TRIM);
                    if (word.Length <= 0 || WORD_BLACKLIST.Contains(word)) continue;
                    int times = 0;
                    userWordList.TryGetValue(word, out times);
                    userWordList[word] = times + 1;

                }
            }

            Console.WriteLine("Printing results");
            await Context.Channel.SendMessageAsync("TOP " + leadCount + " WORDS FOR EACH USER FOR THE PAST " + depth + " MESSAGES ON " + chan.Name + ":");

            foreach (IUser user in bigList.Keys)
            {
                Dictionary<string, int> wordList = bigList[user];
                var toSort = wordList.ToList();

                toSort.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
                toSort.Reverse();

                string toPrint = user.Username + "\'s favorite words:";
                for (int i = 0; (i < leadCount && i < toSort.Count); i++)
                {
                    KeyValuePair<string, int> wordPair = toSort[i];
                    toPrint += "\n\t" + (i + 1) + ". " + wordPair.Key + ": " + wordPair.Value;
                }
                await Context.Channel.SendMessageAsync(toPrint);
                await Task.Delay(500);
            }
            Console.WriteLine("FWord complete");
        }

        [Command("FWord"), Summary("Displays frequency of words")]
        public async Task FavoriteWord([Summary("The channel get data from")] IMessageChannel chan)
        {
            await FavoriteWord(chan, MAX_DEPTH, LEADERBOARD_COUNT);
        }

        [Command("FWord"), Summary("Displays frequency of words")]
        public async Task FavoriteWord()
        {
            await FavoriteWord(Context.Channel, MAX_DEPTH, LEADERBOARD_COUNT);
        }


    }




}
