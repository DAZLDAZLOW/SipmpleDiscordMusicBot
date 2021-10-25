using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using YoutubeExplode;
using YoutubeExplode.Converter;
using System;
using MusicBot.Modules;

namespace MusicBot.Services
{
    public class AudioService : CustomService 
    {
        private readonly string DownloadPath = $"{Directory.GetCurrentDirectory()}\\MusicTemp\\TempAudio.mp3"; //Path to TempMusic
        private readonly int SecondsToChoose = 60; //Seconds before choose will closed
        private readonly QueueManager QM = new();
        private YoutubeClient YTClient = new();
        private IAudioClient CurrentClient;
        private IVoiceChannel CurrentVoice;
        private bool ChoosenSwitch = false;
        private int ChoosenNumber = 0;
       public AudioService()
        {
            if (!Directory.Exists("MusicTemp")) Directory.CreateDirectory("MusicTemp");
            foreach (var file in Directory.GetFiles($"{Directory.GetCurrentDirectory()}\\MusicTemp")) File.Delete(file); //Clears temp Directory
        }

        public async Task PlayYoutubeAsync(string url, IVoiceChannel target, IMessageChannel channel)// Play Command
        {
            try
            {
                if (await YTClient.Videos.GetAsync(url) != null)
                {
                    QM.AddSong(url);
                    await channel.SendMessageAsync($"Song added to queue! Number in Queue:{QM.GetQueueCount()}");
                    if (!QM.PlayingSongIsNotNull()) await DownloadAndPlayAsync(QM.NextSong(), target);
                }
                else await channel.SendMessageAsync($"Cant find downloadable link!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await channel.SendMessageAsync($"Wrong url!({url})");
                await SkipAsync(channel);
            }
        }

        public async Task SkipAsync(IMessageChannel channel) //Skip Command
        {
            if (CurrentClient == null || CurrentVoice == null) return;
            SongEnded();
            await channel.SendMessageAsync($"Song skipped. Now Playing {QM.GetCurrentSongUrl()}");
        }

        public void Stop() //Stop Command
        {
            if (CurrentClient != null)
            {
                CurrentClient.StopAsync().GetAwaiter().GetResult();
                CurrentClient = null;
            }
            if (CurrentVoice != null)
            {
                CurrentVoice.DisconnectAsync().GetAwaiter().GetResult();
                CurrentVoice = null;
            }
            QM.Reset();
            try { if (File.Exists(DownloadPath)) File.Delete(DownloadPath); } catch { }
        }

        public async Task GetNextSongAsync(IMessageChannel channel) => await channel.SendMessageAsync("Next song is "+QM.GetNextSong()); //Next Command
        public async Task GetCurrentSongAsync(IMessageChannel channel) => await channel.SendMessageAsync("Now playing is " + QM.GetCurrentSongUrl());//Now Command
        public void Choose(object numObj)//Choose Command
        {
            int num = (int)numObj;
            ChoosenSwitch = true;
            ChoosenNumber = num;
        }

        public async Task SearchAsync(IVoiceChannel target, IMessageChannel channel,string[] query)//Search Command
        {
            CancellationTokenSource CSource = new();
            ChoosenNumber = 0;
            ChoosenSwitch = false;
            System.Text.StringBuilder sb = new("");
            foreach (string st in query) sb.Append($"{st} ");
            await channel.SendMessageAsync(":mag: Wait, im searching...");
            var searchResultT = YTClient.Search.GetVideosAsync(sb.ToString(), CSource.Token);
            SearchVideoInfo[] searchResult = new SearchVideoInfo[10];
            int temp = 0;
            await foreach (var item in searchResultT)
            {
                searchResult[temp] = new SearchVideoInfo(item.Duration, item.Title,item.Url);
                if (temp < 9) temp++; else { CSource.Cancel(); break; }
            }
            if (temp < 1)
            {
                await channel.SendMessageAsync(":x: Cant find videos in this query!");
                return;
            }
            //Start Embed Build
            var emBuilder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName("Youtube Search"),
                Color = Color.Red,
                Title = "Search Results:"
            };

            for (int i = 0; i < temp + 1; i++)
            {
                string secs = (searchResult[i].Duration.Value.Seconds % 60) > 10 ? (searchResult[i].Duration.Value.Seconds % 60).ToString() : "0" + searchResult[i].Duration.Value.Seconds % 60;
                emBuilder.AddField($"{i + 1}. {searchResult[i].Title}",
                    $"({searchResult[i].Duration.Value.Minutes}:{secs})");
            }
            emBuilder.Footer = new EmbedFooterBuilder().WithText($"Choose video by sending !choose *number*(!c)({SecondsToChoose} seconds to choose)");
            await channel.SendMessageAsync(embed: emBuilder.Build());
            //End Embed Build
            int timer = 0;
            while (!ChoosenSwitch && ++timer < SecondsToChoose) Thread.Sleep(1000);
            if (ChoosenSwitch && ChoosenNumber > 0) await DownloadAndPlayAsync(searchResult[ChoosenNumber - 1].Url, target);
            else await channel.SendMessageAsync(":x: Search Time is Out!");
        }

        private async Task DownloadAndPlayAsync(string url, IVoiceChannel target)
        {
            if (CurrentVoice != target)   CurrentVoice = target;
            if (CurrentClient == null) CurrentClient = await target.ConnectAsync();
            try{  await YTClient.Videos.DownloadAsync(url, DownloadPath); }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR]: Download Error:\n" + ex.Message);
                if (QM.GetCurrentSongUrl() == url) { SongEnded(); Console.WriteLine("Test"); }
                return;
            }
            using (var ffmpeg = CreateProcess(DownloadPath))
            using (var stream = CurrentClient.CreatePCMStream(AudioApplication.Music))
            {
               await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream);
            }
            try { if (File.Exists(DownloadPath)) File.Delete(DownloadPath); } catch { }
            SongEnded();
        }

        private void SongEnded()
        {
            Console.WriteLine("Song is ended");
            if (QM.GetNextSong() != "Nothing") DownloadAndPlayAsync(QM.NextSong(), CurrentVoice).GetAwaiter();
            else Stop();
        }

        private class SearchVideoInfo
        {
            public readonly string Url;
            public readonly TimeSpan? Duration;
            public readonly string Title;
            public SearchVideoInfo(TimeSpan? duration, string title,string url)
            {
                Duration = duration;
                Title = title;
                Url = url;
            }
        }

        private Process CreateProcess(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }
}
