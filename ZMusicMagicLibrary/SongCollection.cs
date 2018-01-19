using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMusicMagicLibrary
{
    public enum SongCollectionType { Base, Overworld, Indoor, Ending };

    public class SongCollection
    {
        public List<Song> Songs { get; set; } = new List<Song>();
        public Dictionary<int, CallLoopPart> LoopParts { get; set; } = new Dictionary<int, CallLoopPart>();
        public string DisplayName { get; set; }
        public SongCollectionType SongCollectionType { get; set; }
        public NSPC.NSPC NSPC { get; set; }

        public SongCollection(SongCollectionType type)
        {
            this.SongCollectionType = type;

            switch(type)
            {
                case SongCollectionType.Base:
                    DisplayName = "Startup Songs";
                    break;
                case SongCollectionType.Overworld:
                    DisplayName = "Overworld Songs";
                    break;
                case SongCollectionType.Indoor:
                    DisplayName = "Indoor Songs";
                    break;
                case SongCollectionType.Ending:
                    DisplayName = "Ending Songs";
                    break;
            }
        }

        public void LoadFromNspc(NSPC.NSPC nspc)
        {
            NSPC = nspc;

            LoadSongs(nspc);

            LoadLoopParts(nspc);
        }

        void LoadSongs(NSPC.NSPC nspc)
        {
            int songIndex = 1; // why is this game 1 based? (0 will do strange things)
            foreach (var s in nspc.Songs)
            {
                var song = new Song();
                song.SongIndex = songIndex;
                songIndex++;

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

        public void FixDurations()
        {
            foreach(var s in Songs)
            {
                s.FixDurations();
            }
        }

        static readonly string[] defaultBaseSongNames = { "Title", "World Map", "Beginning", "Rabbit", "Forest", "Intro", "Town", "Warp", "Dark World", "Master Sword", "File Select", "Soldier", "Mountain", "Shop", "Fanfare", "Song 16", "Song 17", "Song 18", "Song 19", "Song 20", "Song 21", "Song 22", "Song 23", "Song 24", "Song 25", "Song 26", "Song 27" };
        static readonly string[] defaultIndoorSongNames = { "Song 1", "Song 2", "Song 3", "Song 4", "Song 5", "Song 6", "Song 7", "Song 8", "Song 9", "Song 10", "File Select", "Song 12", "Song 13", "Song 14", "Song 15", "Castle", "Palace", "Cave", "Clear", "Church", "Boss", "Dungeon", "Psychic", "Secret Way", "Rescue", "Crystal", "Fountain", "Pyramid", "Kill Agahnim", "Ganon Room", "Last Boss", "Song 32", "Song 33", "Song 34", "Song 35" };
        static readonly string[] defaultEndingSongNames = { "Song 1", "Song 2", "Song 3", "Song 4", "Song 5", "Song 6", "Song 7", "Song 8", "Song 9", "Song 10", "Song 11", "Song 12", "Song 13", "Song 14", "Song 15", "Song 16", "Song 17", "Song 18", "Song 19", "Song 20", "Song 21", "Song 22", "Song 23", "Song 24", "Song 25", "Song 26", "Song 27", "Song 28", "Song 29", "Song 30", "Song 31", "Triforce", "Ending", "Staff", "Song 35" };
        public void LoadDefaultSongNames()
        {
            for(int i = 0; i < Songs.Count; ++i)
            {
                switch(this.SongCollectionType)
                {
                    case SongCollectionType.Base:
                    case SongCollectionType.Overworld:
                        Songs[i].DisplayName = defaultBaseSongNames[i];
                        break;
                    case SongCollectionType.Indoor:
                        Songs[i].DisplayName = defaultIndoorSongNames[i];
                        break;
                    case SongCollectionType.Ending:
                        Songs[i].DisplayName = defaultEndingSongNames[i];
                        break;
                }
            }
        }
    }
}
