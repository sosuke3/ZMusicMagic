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

        public List<Track> Tracks { get; set; } = new List<Track>();

        public void LoadTrackAddresses(byte[] aramBuffer)
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
                Tracks.Add(new Track() { Address = address });
            }
        }

        public void LoadTrackData(byte[] aramBuffer, List<Song> songs)
        {
            // we need to know about all the songs/parts to know where things end because 00 isn't always at the end of a track
            foreach(var t in Tracks.Where(x => x.Address != 0).ToList())
            {
                int nextAddress = FindNextAddress(t.Address, songs);

                if(nextAddress > t.Address)
                {
                    //for(int i = t; i < nextAddress; i++)
                    //{
                    //    if(aramBuffer[i] == 0x00)
                    //    {
                    //        nextAddress = i;
                    //        break;
                    //    }
                    //}

                    t.RawTrackData = new byte[nextAddress - t.Address];
                    Array.Copy(aramBuffer, t.Address, t.RawTrackData, 0, t.RawTrackData.Length);
                }
            }
        }

        int FindNextAddress(int address, List<Song> songs)
        {
            int nextAddress = 0xFFFF;

            foreach (var s in songs)
            {
                foreach(var p in s.Parts)
                {
                    foreach(var t in p.Tracks)
                    {
                        if(t.Address > address && t.Address < nextAddress)
                        {
                            nextAddress = t.Address;
                        }
                    }
                }
            }

            return nextAddress;
        }
    }
}
