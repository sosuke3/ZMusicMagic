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
        public Dictionary<int, CallLoopPart> LoopParts { get; set; } = new Dictionary<int, CallLoopPart>();

        public void LoadFromNspc(NSPC.NSPC nspc)
        {
            LoadSongs(nspc);

            LoadLoopParts(nspc);
        }

        void LoadSongs(NSPC.NSPC nspc)
        {
            foreach (var s in nspc.Songs)
            {
                var song = new Song();

                foreach (var p in s.Parts)
                {
                    var part = new Part();

                    foreach (var t in p.Tracks)
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

        void LoadLoopParts(NSPC.NSPC nspc)
        {
            foreach(var s in Songs)
            {
                foreach(var p in s.Parts)
                {
                    foreach(var c in p.Channels)
                    {
                        foreach(var cmd in c.Commands.OfType<CallLoopCommand>().ToList())
                        {
                            if(false == this.LoopParts.ContainsKey(cmd.LoopAddress))
                            {
                                CallLoopPart loopPart = new CallLoopPart();
                                loopPart.Address = cmd.LoopAddress;
                                loopPart.Commands = ChannelCommand.GetChannelCommandsFromRawData(nspc.aramBuffer, loopPart.Address);
                                this.LoopParts.Add(loopPart.Address, loopPart);
                                cmd.LoopPart = loopPart;
                            }
                            else
                            {
                                cmd.LoopPart = this.LoopParts[cmd.LoopAddress];
                            }
                        }
                    }
                }
            }
        }
    }
}
