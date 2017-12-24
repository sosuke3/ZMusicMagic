using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMusicMagicLibrary.NSPC
{
    public class SongPart
    {
        public int PartPointerAddress { get; set; } // address in the pointer table, used for figuring out the loop point if one exists

        public int SongPartTrackTableAddress { get; set; } // address of the actual song part data pointer table

        public int LoopCount { get; set; }

        public int GotoPartAddress { get; set; }
        public SongPart NextPart { get; set; } // do I need this?

        public List<int> TrackAddresses { get; set; } = new List<int>();

        public void LoadTracks(byte[] aramBuffer)
        {
            if (SongPartTrackTableAddress <= 0x00FF)
            {
                // either a stop, or goto "part" so skip it
                return;
            }

            // finally we get to something easy, there should always be a table with 8 2byte addresses to each of the tracks
            for(int i = 0; i < 8; i++)
            {
                int address = aramBuffer[SongPartTrackTableAddress + i * 2] | (aramBuffer[SongPartTrackTableAddress + i * 2 + 1] << 8);
                TrackAddresses.Add(address);
            }
        }
    }
}
