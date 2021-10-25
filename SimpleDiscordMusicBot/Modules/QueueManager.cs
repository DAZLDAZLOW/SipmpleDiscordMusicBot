using System.Collections.Generic;

namespace MusicBot.Modules //Random comment to test git
{
    public class QueueManager //Manage queue of Youtube Songs
    {
        private readonly Queue<string> _songsInQueue = new();
        private string _playingSong;
        public void AddSong(string url) => _songsInQueue.Enqueue(url); //Add Song to Queue 
        public string NextSong() //Returns url of next song in queue
        {
            if (_songsInQueue.Count == 0) return "";
            _playingSong = _songsInQueue.Dequeue();
            return _playingSong;
        }
        public string SkipSong()//Skip playing song and return url of next song in queue
        {
            if (_songsInQueue.Count > 0)
            {
                _playingSong =  _songsInQueue.Dequeue();
                return _playingSong;            
            }
            else return "";
        }
        public int GetQueueCount() => (_playingSong != null) ? _songsInQueue.Count + 1 : _songsInQueue.Count;//Returns number of song in play order
        public string GetCurrentSongUrl() => _playingSong != null?_playingSong:"Nothing";
        public string GetNextSong() => _songsInQueue.Count > 0 ? _songsInQueue.Peek(): "Nothing";
        public bool PlayingSongIsNotNull() => _playingSong != null; //Returns true if _playingSong Exists
        public void Reset() // Resets all values of Queue Manager 
        {
            _songsInQueue.Clear();
            _playingSong = null;
        }
    }
}
