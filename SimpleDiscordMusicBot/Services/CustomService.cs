using Discord;
using MusicBot.Modules;

namespace MusicBot.Services
{
    public enum E_LogOutput { Console, Reply, Playing };

    /**
     * CustomService
     * Class that handles serves as a wrapper for services.
     * Add shared functionality here and shared properties between all services.
     * This should be paired with the CustomModule to use these functions.
     */
    public class CustomService
    {
        // We have a reference to the parent module to perform actions 
        // like replying and setting the current game properly.
        private CustomModule m_ParentModule = null;

        // This should always be called in the module constructor to 
        // provide a direct reference to the parent module.
        public void SetParentModule(CustomModule parent) { m_ParentModule = parent; }

        // Replies in the text channel using the parent module and optional embed.
        protected async void DiscordReply(string s, EmbedBuilder emb = null)
        {
            if (m_ParentModule == null) return;
            if (emb != null)
                await m_ParentModule.ServiceReplyAsync(s, emb);
            else
                await m_ParentModule.ServiceReplyAsync(s);
        }

        //  Sets the playing status using the parent module.
        protected async void DiscordPlaying(string s)
        {
            if (m_ParentModule == null) return;
            await m_ParentModule.ServicePlayingAsync(s);
        }

        // A Custom logger which can send messages to 
        // console, reply in module, or set to playing.
        // By default, we log everything to the console.
        // TODO: Configure as OR flags to set multiple options.

    }
}
