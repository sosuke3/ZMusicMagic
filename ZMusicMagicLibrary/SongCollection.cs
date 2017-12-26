using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMusicMagicLibrary
{
    public class SongCollection
    {
        public List<Song> Songs { get; set; } = new List<Song>();

        public void LoadFromNspc(NSPC.NSPC nspc)
        {
            foreach(var s in nspc.Songs)
            {
                var song = new Song();

                foreach(var p in s.Parts)
                {
                    var part = new Part();

                    foreach(var t in p.Tracks)
                    {
                        var channel = new Channel();

                        channel.LoadFromNSPCTrack(t);

                        part.Channels.Add(channel);
                    }

                    song.Parts.Add(part);
                }

                Songs.Add(song);
            }
        }
    }
}
