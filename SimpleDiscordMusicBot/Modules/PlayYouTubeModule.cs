using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MusicBot.Services;

namespace MusicBot.Modules
{
    public class PlayYouTubeModule : ModuleBase<SocketCommandContext>
    { 
        private readonly AudioService _service;

        public PlayYouTubeModule(AudioService service)
        {
            _service = service;
        }

        [Command("help",RunMode = RunMode.Async)]
        [Alias("h")]

        public async Task HelpAsync()
        {
           await Context.Channel.SendMessageAsync("Commands:\nPlay(p) *url* - Plays audio from youtube video.\nSearch(src) *Search Query* - Search videos on youtube by query.\nStop - Disconnect music bot.\nNow/Next - Displays Current/Next song in queue.");
        }

        [Command("play",RunMode = RunMode.Async)]
        [Alias("p")]
        [Summary("Play song from Youtube by url")]
        public async Task PlayURLAsync(string URL)
        {
            if ((Context.User as IVoiceState).VoiceChannel != null)
                await _service.PlayYoutubeAsync(URL, (Context.User as IVoiceState).VoiceChannel, Context.Channel);
            else await Context.Channel.SendMessageAsync(":x: You must be in Voice Channel!");
        }

        [Command("search", RunMode = RunMode.Async)]
        [Alias("src")]
        [Summary("Search ten videos on youtube")]
        public async Task SearchAsync(params string[] query) => await _service.SearchAsync((Context.User as IVoiceState).VoiceChannel, Context.Channel, query);

        [Command("choose")]
        [Alias("c","ch")]
        [Summary("Chooses current result")]
        public async Task ChooseAsync(string number)
        {
            int num;
            if(int.TryParse(number,out num) && num > 0 && num< 11)
            {
               _service.Choose(num);
            }
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Summary("Stops playing music")]
        public async Task StopPlayingAsync() => await Task.Run(_service.Stop);

        [Command("skip", RunMode = RunMode.Async)]
        [Summary("Skips playing music")]
        public async Task SkipAsync() => await _service.SkipAsync(Context.Channel);

        [Command("next", RunMode = RunMode.Async)]
        [Summary("Displays next song in queue")]
        public async Task NextAsync() => await _service.GetNextSongAsync(Context.Channel);

        [Command("now", RunMode = RunMode.Async)]
        [Summary("Displays playings song")]
        public async Task NowAsync() => await _service.GetCurrentSongAsync(Context.Channel);
    }
}
